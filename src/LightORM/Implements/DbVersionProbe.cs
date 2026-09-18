using System.Collections.Concurrent;
using System.Data.Common;
using System.Text.RegularExpressions;
using System.Threading;
using LightORM.Models;

namespace LightORM.Implements;

/// <summary>
/// 数据库版本探测
/// <para>
/// provider 在<b>构造期</b>调用 <see cref="Start"/> 发起一次后台探测，此后任意位置（含 SQL 构建期）
/// 读 <see cref="Value"/> 即可拿到结果；尚未完成则等待其完成。
/// </para>
/// </summary>
public sealed class DbVersionProbe
{
    private const int DefaultTimeoutSeconds = 2;
    private const int DefaultAttempts = 3;
    private const int RetryBackoffMs = 1000;

    /// <summary>
    /// (目标, 连接串) → 探测状态。单飞：同目标的多个 provider 实例共享同一次探测，
    /// 也共享后台重试的自愈结果。条目不回收——数量由不同连接串个数决定，正常应用是个位数。
    /// </summary>
    private static readonly ConcurrentDictionary<string, Lazy<ProbeState>> probes =
        new(StringComparer.Ordinal);

    /// <summary>
    /// 只取版本串的前导数字段：PG 会返回 "16.4 (Debian 16.4-1.pgdg120+1)" 这类带后缀的串。
    /// 必须<b>最多四段</b>——<see cref="Version"/> 只接受 2~4 段，而 Oracle 的 ServerVersion 是
    /// "21.3.0.0.0"（五段），多截一段会导致 TryParse 失败并静默退化成基线。
    /// </summary>
    private static readonly Regex leadingVersion = new(@"^\s*(\d+(?:\.\d+){0,3})", RegexOptions.Compiled);

    private readonly ProbeState? state;
    private volatile Version? overridden;
    private volatile bool hasOverride;

    private DbVersionProbe(ProbeState? state) => this.state = state;

    /// <summary>
    /// 不探测，恒返回版本未知（无连接串的静态实例、纯 SQL 生成场景）。
    /// </summary>
    public static DbVersionProbe None => new(null);

    /// <summary>
    /// 发起后台版本探测。
    /// </summary>
    /// <param name="target">探测目标的标识，同一标识 + 同一连接串只探测一次（用 <see cref="DbBaseType.Name"/>）。</param>
    /// <param name="factory">该 provider 使用的驱动工厂，探测连接由它创建（尊重 <c>OverrideDbProviderFactory</c>）。</param>
    /// <param name="connectionString">连接串；仅用于探测，不影响业务连接。</param>
    /// <param name="prepareConnectString">
    /// <b>由方言提供</b>：把探测用的连接串改写成"带秒级连接超时"的版本；入参为 (原连接串, 超时秒数)，
    /// 返回 null 表示不改写。<b>核心只决定"探测必须秒级返回"这个策略，怎么表达超时是方言的私有知识。</b>
    /// </param>
    /// <param name="timeoutSeconds">探测连接的连接超时上限，覆盖连接串里的原值。</param>
    /// <param name="attempts">探测尝试次数；首次失败后的重试在后台进行，不增加读取侧等待。</param>
    /// <param name="openBeforeRead">
    /// 读取 <see cref="DbConnection.ServerVersion"/> 前是否必须先 Open。
    /// 除 SQLite 外所有驱动在关闭连接上读该属性都会抛异常，故默认 true；
    /// SQLite 传 false——它未 Open 即可读，且对其执行 Open 会在文件不存在时<b>建出一个空库文件</b>。
    /// </param>
    public static DbVersionProbe Start(string target,
        DbProviderFactory factory,
        string connectionString,
        Func<string, int, string?>? prepareConnectString,
        int timeoutSeconds = DefaultTimeoutSeconds,
        int attempts = DefaultAttempts,
        bool openBeforeRead = true)
    {
        var key = $"{target}|{connectionString}";
        var lazy = probes.GetOrAdd(key, _ => new Lazy<ProbeState>(
            () => StartProbe(factory, connectionString, prepareConnectString, timeoutSeconds, attempts, openBeforeRead),
            LazyThreadSafetyMode.ExecutionAndPublication));
        return new DbVersionProbe(lazy.Value);
    }

    /// <summary>显式指定版本，优先级高于探测结果。</summary>
    public void Override(Version? version)
    {
        overridden = version;
        hasOverride = true;
    }

    /// <summary>是否已显式指定版本。</summary>
    public bool HasOverride => hasOverride;

    /// <summary>探测是否已就绪（诊断用，不做等待）。</summary>
    public bool IsResolved => hasOverride
        || state is null
        || state.Healed is not null
        || state.First.Status == TaskStatus.RanToCompletion;

    /// <summary>
    /// 版本结果。若后台探测尚未完成会等待，等待上限为一次连接超时；
    /// 失败后的重试在后台进行，不占用读取侧时间。
    /// </summary>
    public DbServerVersion Value
    {
        get
        {
            if (hasOverride)
            {
                return new DbServerVersion(null, overridden, CapabilitySource.Explicit);
            }

            if (state is null)
            {
                return new DbServerVersion(null, null, CapabilitySource.Assumed);
            }

            // 后台重试若已自愈，优先用新结果（同一连接串的所有调用方共享，见 ProbeState）。
            var healed = state.Healed;
            if (healed is not null)
            {
                return healed.GetAwaiter().GetResult();
            }

            // First 永不 fault，故不需要 catch
            return state.First.GetAwaiter().GetResult();
        }
    }

    private static ProbeState StartProbe(DbProviderFactory factory,
        string connectionString,
        Func<string, int, string?>? prepareConnectString,
        int timeoutSeconds,
        int attempts,
        bool openBeforeRead)
    {
        var first = TryOnceAsync(factory, connectionString, prepareConnectString, timeoutSeconds, openBeforeRead);
        var state = new ProbeState(first);
        if (attempts > 1)
        {
            // 启动期数据库未就绪是最常见情形，靠有限次退避重试自愈。
            _ = RetryAsync(state, factory, connectionString, prepareConnectString, timeoutSeconds, attempts, openBeforeRead);
        }
        return state;
    }

    /// <summary>单次探测，<b>永不抛异常</b>：连不上 / 驱动未实现 ServerVersion 时返回 Assumed。</summary>
    private static async Task<DbServerVersion> TryOnceAsync(DbProviderFactory factory,
        string connectionString,
        Func<string, int, string?>? prepareConnectString,
        int timeoutSeconds,
        bool openBeforeRead)
    {
        try
        {
            var raw = await ReadServerVersionAsync(factory, connectionString, prepareConnectString, timeoutSeconds, openBeforeRead)
                .ConfigureAwait(false);
            Console.WriteLine($"database version {raw}");
            return new DbServerVersion(raw, ParseVersion(raw), CapabilitySource.Probed);
        }
        catch(Exception ex)
        {
            Console.Write("database version error ");
            Console.WriteLine(ex.Message);
            return new DbServerVersion(null, null, CapabilitySource.Assumed);
        }
    }

    private static async Task RetryAsync(ProbeState state,
        DbProviderFactory factory,
        string connectionString,
        Func<string, int, string?>? prepareConnectString,
        int timeoutSeconds,
        int attempts,
        bool openBeforeRead)
    {
        try
        {
            var current = await state.First.ConfigureAwait(false);
            if (current.IsKnown)
            {
                return;
            }

            for (var attempt = 2; attempt <= attempts; attempt++)
            {
                try
                {
                    await Task.Delay(RetryBackoffMs * (attempt - 1)).ConfigureAwait(false);
                }
                catch
                {
                    // 忽略
                }

                var next = await TryOnceAsync(factory, connectionString, prepareConnectString, timeoutSeconds, openBeforeRead)
                    .ConfigureAwait(false);
                if (next.IsKnown)
                {
                    state.Healed = Task.FromResult(next);
                    return;
                }
            }
        }
        catch
        {
            // 后台任务绝不外抛
        }
    }

    private static async Task<string?> ReadServerVersionAsync(DbProviderFactory factory,
        string connectionString,
        Func<string, int, string?>? prepareConnectString,
        int timeoutSeconds,
        bool openBeforeRead)
    {
        using var conn = factory.CreateConnection()
            ?? throw new InvalidOperationException($"DbProviderFactory 未能创建连接：{factory.GetType().Name}");
        conn.ConnectionString = PrepareConnectString(connectionString, prepareConnectString, timeoutSeconds);
        if (openBeforeRead)
        {
            await conn.OpenAsync().ConfigureAwait(false);
        }
        return conn.ServerVersion;
    }

    /// <summary>
    /// 问方言"这条连接串怎么把连接超时压到秒级"——实测驱动默认超时 SqlServer 15s、其余 2~4s，太慢。
    /// <b>改写方式（用哪个键名 / 哪个强类型属性 / 是否值得改写）是方言的私有知识</b>，
    /// 核心只负责调用与兜底：委托为 null、返回 null 或抛异常时，一律沿用原连接串。
    /// </summary>
    private static string PrepareConnectString(string connectionString,
        Func<string, int, string?>? prepare,
        int timeoutSeconds)
    {
        if (prepare is null)
        {
            return connectionString;
        }

        try
        {
            return prepare(connectionString, timeoutSeconds) ?? connectionString;
        }
        catch
        {
            // 连接串本身非法等 → 不改写，探测照常进行（失败也只会得到 Assumed，不会外抛）
            return connectionString;
        }
    }

    private static Version? ParseVersion(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
        {
            return null;
        }

        var match = leadingVersion.Match(raw);
        if (!match.Success)
        {
            return null;
        }

        return Version.TryParse(match.Groups[1].Value, out var version) ? version : null;
    }

    /// <summary>
    /// 同一 (目标, 连接串) 的共享探测状态。所有调用方共用一份，
    /// 故后台自愈的结果对所有调用方可见。
    /// </summary>
    private sealed class ProbeState(Task<DbServerVersion> first)
    {
        /// <summary>读取侧等待的目标：只含首次尝试，上限一个连接超时。</summary>
        internal Task<DbServerVersion> First { get; } = first;

        /// <summary>后台重试成功后的结果；为 null 表示尚未自愈。</summary>
        internal volatile Task<DbServerVersion>? Healed;
    }
}

using System.Data.Common;

namespace LightORM.Implements;

/// <summary>
/// 数据库能力档案的公共骨架：<b>探测机制全部在这里，方言只提供差异</b>。
/// <para>
/// 方言需要实现两样：<see cref="ProbeTarget"/> 与 <see cref="MapFeatures"/>；
/// <see cref="PrepareProbeConnectString"/> / <see cref="OpenBeforeRead"/> 有默认值可按需覆盖；
/// <see cref="Baseline"/> / <see cref="ForVersion"/> / <see cref="Start"/> 以及所有读取成员都由基类提供。
/// </para>
/// <para>
/// <b>只有"同一 SQL 片段存在两套写法、需要按版本二选一"的方言才需要有这个类</b>——探测的唯一目的就是喂
/// <see cref="Features"/>。没有任何版本相关分支的方言不要接入：探测结果无处可用
/// </para>
/// net462 / netstandard2.0 的运行时<b>不支持接口中的静态抽象成员</b>（static abstract 需要 .NET 7+ 运行时，net462 上不可用）
/// </summary>
public abstract class DbCapabilities<TDatabase, TDatabaseFeatures>
    where TDatabase : DbCapabilities<TDatabase, TDatabaseFeatures>, new()
{
    /// <summary>
    /// 探测状态。只由基类的静态工厂写入；读取请走 <see cref="ServerVersion"/> 等公开成员。
    /// </summary>
    protected DbVersionProbe Probe { get; set; } = DbVersionProbe.None;

    /// <summary>
    /// 探测的单飞键，用 <see cref="DbBaseType.Name"/>（同一键 + 同一连接串全进程只探测一次）。
    /// </summary>
    protected abstract string ProbeTarget { get; }

    /// <summary>
    /// 把探测用的连接串改写成"带秒级连接超时"的版本；返回 null 表示不改写。
    /// <para>
    /// <b>核心只给策略（探测必须秒级返回），怎么表达超时是方言的私有知识</b>：SqlServer 用
    /// <c>SqlConnectionStringBuilder.ConnectTimeout</c>、Oracle 用 <c>ConnectionTimeout</c>、MySQL 用 <c>ConnectionTimeout</c>(uint)……
    /// 按"超时键名"字符串试探区分不了语义（多个驱动同时接受 <c>Command Timeout</c> 且读回值相等），故交给方言。
    /// </para>
    /// 默认不改写；返回 null 或抛异常时，核心沿用原连接串让探测自己去失败。
    /// </summary>
    protected virtual string? PrepareProbeConnectString(string connectionString, int timeoutSeconds) => null;

    /// <summary>
    /// 读取 <see cref="DbConnection.ServerVersion"/> 前是否必须先 Open。
    /// 除 SQLite 外所有驱动在关闭连接上读该属性都会抛异常，故默认 true；
    /// SQLite 覆盖为 false——它未 Open 即可读，且对其 Open 会在文件不存在时建出一个空库文件。
    /// </summary>
    protected virtual bool OpenBeforeRead => true;

    /// <summary>
    /// 版本 → 能力位。<b>版本不可用（未探测 / 探测失败 / 未启用探测）时的基线策略由方言自己定</b>：
    /// 可以保守（退到全版本可用的旧写法），也可以乐观（按完整功能生成），取决于该方言的能力位是否
    /// "新写法在旧库直接失败"。直接比版本分量，不要构造 <c>Version</c> 实例做比较。
    /// </summary>
    public abstract TDatabaseFeatures MapFeatures(Version? version);

    /// <summary>服务端版本，含原始串与来源（Explicit / Probed / Assumed）。</summary>
    public DbServerVersion ServerVersion => Probe.Value;

    public Version? Version => ServerVersion.Parsed;

    public CapabilitySource Source => ServerVersion.Source;

    /// <summary>探测是否已就绪（诊断用，不做等待）。</summary>
    public bool IsResolved => Probe.IsResolved;

    /// <summary>能力位。由版本推导，故必须与 SQL 生成期的读取保持一致。</summary>
    public TDatabaseFeatures Features => MapFeatures(Version);

    /// <summary>显式指定版本，优先级高于探测结果。</summary>
    public void Override(Version? version) => Probe.Override(version);

    /// <summary>不探测（未启用探测 / 纯 SQL 生成 / 静态实例），版本视为未知。</summary>
    public static TDatabase Baseline { get; } = ForProbe(new TDatabase(), DbVersionProbe.None);

    /// <summary>使用显式指定的版本，<b>不发起后台探测</b>。</summary>
    public static TDatabase ForVersion(Version version) => ForProbe(new TDatabase(), version);

    /// <summary>
    /// 构造期发起后台探测。三条不探测的出口，结果都落到 <see cref="Baseline"/>：
    /// <list type="bullet">
    /// <item><paramref name="specificVersion"/> 已给定——用户既然指定了版本，就没有连库的必要；</item>
    /// <item><paramref name="detectVersion"/> 为 false——用户主动关闭探测（典型的 ToSql 纯生成场景）；</item>
    /// <item><paramref name="connectionString"/> 为空——没有可探测的目标。</item>
    /// </list>
    /// 探测本身失败（连不上 / 未就绪）与上述出口同结果：版本视为未知，能力位落到方言的基线策略。
    /// </summary>
    public static TDatabase Start(DbProviderFactory factory,
        string connectionString,
        Version? specificVersion = null,
        bool detectVersion = true)
    {
        if (specificVersion is not null)
        {
            return ForVersion(specificVersion);
        }

        if (!detectVersion || string.IsNullOrWhiteSpace(connectionString))
        {
            return Baseline;
        }

        // 借一个空实例读出方言元数据（探测键 / 连接串改写 / 是否需先 Open），探测结果也写回它。
        var metadata = new TDatabase();
        var probe = DbVersionProbe.Start(metadata.ProbeTarget,
            factory,
            connectionString,
            metadata.PrepareProbeConnectString,
            openBeforeRead: metadata.OpenBeforeRead);

        return ForProbe(metadata, probe);
    }

    /// <summary>把探测状态写进实例（<paramref name="probe"/> 为 None 即"不探测"）。</summary>
    private static TDatabase ForProbe(TDatabase capabilities, DbVersionProbe probe)
    {
        capabilities.Probe = probe;
        return capabilities;
    }

    /// <summary>在空实例上写显式版本（等同 <see cref="Override"/>，供静态工厂使用）。</summary>
    private static TDatabase ForProbe(TDatabase capabilities, Version version)
    {
        capabilities.Probe = DbVersionProbe.None;
        capabilities.Override(version);
        return capabilities;
    }
}

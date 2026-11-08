using LightORM.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace LightORM;

/// <summary>
/// 把配置文件里的数据库条目同步到 LightORM 的全局注册表。
/// <para>
/// 生命周期跟随容器：构造时完成<b>首次加载</b>并订阅 <see cref="IOptionsMonitor{TOptions}.OnChange"/>，
/// 容器 dispose 时退订。不要把这个订阅做成 static 字段 —— 多容器 / 多宿主会互相顶掉，且永不释放。
/// </para>
/// <para>
/// 语义：配置是权威来源。配置项覆盖同 key 的既有 provider（所以改连接串会生效），
/// 配置里被删掉的 key 会被移除；由代码显式 <c>SetDatabase</c> 注册、但不出现在配置里的库不动。
/// </para>
/// </summary>
internal sealed class DatabaseListReloader : IDisposable
{
    private readonly IOptionsMonitor<DatabaseList> monitor;
    private readonly ILogger? logger;
    private readonly IDisposable? subscription;
    private readonly object gate = new();
    private HashSet<string> configuredKeys = new(StringComparer.Ordinal);

    public DatabaseListReloader(IOptionsMonitor<DatabaseList> monitor, IServiceProvider services)
    {
        this.monitor = monitor;
        logger = services.GetService<ILogger<DatabaseListReloader>>();

        // 首次加载：配置写错在这里就暴露，而不是等到第一次查库
        Sync(this.monitor.CurrentValue);

        subscription = monitor.OnChange(list =>
        {
            try
            {
                Sync(list);
            }
            catch (Exception ex)
            {
                // 热重载发生在文件监听线程，不能让异常把它带崩；记录后沿用当前内存状态
                logger?.LogError(ex, "重新加载 LightORM 数据库配置失败，沿用当前已注册的数据库。");
            }
        });
    }

    private void Sync(DatabaseList list)
    {
        lock (gate)
        {
            var next = new HashSet<string>(StringComparer.Ordinal);
            foreach (var item in list.LightOrmDatabases)
            {
                var definition = item.Option;
                if (definition is null)
                {
                    logger?.LogWarning("数据库配置项 '{Key}' 缺少 Option，已跳过。", item.Key);
                    continue;
                }

                if (string.IsNullOrWhiteSpace(definition.Type))
                {
                    logger?.LogWarning("数据库配置项 '{Key}' 缺少 Type，已跳过。", item.Key);
                    continue;
                }

                if (!ExpressionSqlOptions.TryGetCreator(definition.Type, out var create))
                {
                    throw new LightOrmException(
                        $"数据库 '{item.Key}' 的 Type '{definition.Type}' 没有注册创建器。" +
                        $"请先在 AddLightOrm 中调用对应方言的 UseXxx()。已注册：{string.Join(", ", ExpressionSqlOptions.CreatorNames)}");
                }

                // 创建失败（缺连接串等）直接抛：初始加载时让宿主启动失败，比运行期 KeyNotFound 好查
                var provider = create!.Invoke(item);
                ExpressionSqlOptions.ApplyDatabase(item.Key, provider.DbBaseType, provider, replace: true);
                _ = next.Add(item.Key ?? ConstString.Main);
            }

            foreach (var stale in configuredKeys)
            {
                if (!next.Contains(stale))
                {
                    _ = ExpressionSqlOptions.RemoveDatabase(stale);
                }
            }

            configuredKeys = next;
        }
    }

    public void Dispose() => subscription?.Dispose();
}

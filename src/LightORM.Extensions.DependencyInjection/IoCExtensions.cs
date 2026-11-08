using LightORM.ExpressionSql;
using LightORM.Implements;
using LightORM.Interfaces;
using LightORM.Options;
using LightORM.Repository;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using System.Diagnostics.CodeAnalysis;

namespace LightORM;

public static class IoCExtensions
{
    internal static WeakReference<IServiceCollection>? WeakServices { get; set; }

    /// <summary>
    /// 注册 LightORM。
    /// <para>
    /// 容器里存在 <see cref="IConfiguration"/> 且其中包含 <c>LightOrmDatabases</c> 节时，数据库即由配置描述，
    /// 无需在调用处额外传参；没有该节（或没有 <see cref="IConfiguration"/>）时按空列表处理，不报错。
    /// 结构如下（数组，每个元素 = 一个注册的库）：
    /// </para>
    /// <code>
    /// {
    ///   "LightOrmDatabases": [
    ///     {
    ///       "Key": "MainDb",              // 注册名，不写则视为 "MainDb"
    ///       "Option": {
    ///         "Type": "PostgreSQL",           // 必填，须与某个空参 UseXxx() 登记的方言名一致（大小写不敏感）
    ///         "ConnectionString": string,   // 可直接给整串；下面的分散字段会覆盖其中的同名项
    ///         "DataSource": string,         // 别名/描述符：Oracle 的 TNS 或 EZ Connect、Sqlite 的文件路径
    ///         "Server": string,
    ///         "Port": int,               // 不写则用方言默认端口（如 Oracle 1521 / Dameng 5236）
    ///         "UserId": string,
    ///         "Password": string,
    ///         "Database": string,
    ///         "ServiceName": string,        // Oracle 专用，与 Sid 二选一
    ///         "Sid": string,                // Oracle 专用
    ///         "SlaveConnectionStrings": string[],        // 从库
    ///         "IsUseIdentifierQuote": bool,        // 标识符引用
    ///         "SpecificVersion": string,             // 指定服务端版本，给了就不再探测
    ///         "DetectVersion": bool,               // false 时不探测版本
    ///         "DefaultStringLength": int,
    ///         "Settings": { }                      // 方言私有开关，如 PG 的 TableSpace
    ///       }
    ///     }
    ///   ]
    /// }
    /// </code>
    /// <para>
    /// <c>Option.Type</c> 的可选值取决于代码里调用过哪些空参重载
    /// （<c>UseSqlite()</c> / <c>UseMySql()</c> / <c>UseSqlServer()</c> / <c>UsePostgreSQL()</c> /
    /// <c>UseOracle()</c> / <c>UseDameng()</c> / <c>UseKingbaseES()</c>）；未登记的类型会抛异常并列出已支持的方言名。
    /// </para>
    /// <para>
    /// 绑定发生在运行时：由 <see cref="IConfigureOptions{TOptions}"/> 实现从容器取 <see cref="IConfiguration"/>，
    /// 因此 <c>services.AddSingleton&lt;IConfiguration&gt;(...)</c> 在 <c>AddLightOrm</c> 之后注册同样有效。
    /// 首个 <see cref="IExpressionContext"/> 解析时加载并订阅文件变更，之后配置变化会<b>覆盖/新增/移除</b>对应的库。
    /// </para>
    /// </summary>
    /// <param name="services">服务集合。</param>
    /// <param name="options">
    /// 代码方式的配置。与配置文件可共存：代码里 <c>UseXxx(connStr)</c> 先登记实例，
    /// 配置文件中同 <c>Key</c> 的条目随后覆盖它。
    /// </param>
    public static IServiceCollection AddLightOrm(this IServiceCollection services, Action<IExpressionContextSetup> options)
    {
        var builder = new ExpressionOptionBuilder();
        WeakServices = new(services);
        options(builder);

        // 配置驱动注册是"运行时判定"：容器里有 IConfiguration 且含 LightOrmDatabases 节才生效。
        // 绑定源与变更令牌都做成惰性实现，所以这里不需要（也不能）拿 IConfiguration 实例。
        services.AddOptions();
        services.TryAddEnumerable(ServiceDescriptor.Singleton<IConfigureOptions<DatabaseList>, DatabaseListConfigureOptions>());
        services.TryAddSingleton<IOptionsChangeTokenSource<DatabaseList>, ConfigurationReloadTokenSource>();
        // 单例：首次构造时做初始加载并订阅变更，容器 dispose 时退订。
        services.TryAddSingleton<DatabaseListReloader>();

        services.AddScoped(Build);
        services.AddScoped(typeof(ILightOrmRepository<>), typeof(DefaultRepository<>));
        services.AddScoped<IExpressionContext, ExpressionCoreSql>();
        return services;
    }

    private static ExpressionSqlOptions Build(IServiceProvider provider)
    {
        // 触发单例构造：初始加载 + 订阅变更都在 DatabaseListReloader 里完成。
        _ = provider.GetService<DatabaseListReloader>();
        return new(provider.GetServices<IAdoInterceptor>());
    }
}

public static class ExtenIExpressionContextSetup
{
    extension(IExpressionContextSetup setup)
    {
#if NET8_0_OR_GREATER
    public IExpressionContextSetup UseInterceptor<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] T>()
#else
        public IExpressionContextSetup UseInterceptor<T>()
#endif
            where T : AdoInterceptorBase
        {
            var item = typeof(T);
            var nonParameterCtor = item.GetConstructors().FirstOrDefault(c => c.GetParameters().Length == 0);
            if (nonParameterCtor != null)
            {
                // 如果有无参构造器，当作无状态的处理
                if (nonParameterCtor?.Invoke([]) is IAdoInterceptor obj)
                    ExpressionSqlOptions.AddStateLessInterceptor(item, obj);
            }
            else if (IoCExtensions.WeakServices?.TryGetTarget(out var services) == true)
            {
                services?.AddScoped(typeof(IAdoInterceptor), item);
            }

            return setup;
        }


    }
}

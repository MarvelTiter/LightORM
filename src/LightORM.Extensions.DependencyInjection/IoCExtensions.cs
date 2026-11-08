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
    /// 传入 <paramref name="configuration"/> 后，数据库可由配置节 <c>LightOrmDatabases</c> 描述，
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
    /// </summary>
    /// <param name="services">服务集合。</param>
    /// <param name="options">
    /// 代码方式的配置。与配置文件可共存：代码里 <c>UseXxx(connStr)</c> 先登记实例，
    /// 配置文件中同 <c>Key</c> 的条目随后覆盖它。
    /// </param>
    /// <param name="configuration">
    /// 提供 <c>LightOrmDatabases</c> 节的配置。传入后会：
    /// ① 在首次解析 <see cref="IExpressionContext"/> 时按配置注册数据库；
    /// ② 订阅文件变更，配置变化后<b>覆盖/新增/移除</b>对应的库（连接串改动会生效）。
    /// <para>为 null 时整条链路跳过，行为与不使用配置的版本完全一致。</para>
    /// </param>
    public static IServiceCollection AddLightOrm(this IServiceCollection services, Action<IExpressionContextSetup> options, IConfiguration? configuration = null)
    {
        var builder = new ExpressionOptionBuilder();
        WeakServices = new(services);
        options(builder);

        if (configuration is not null)
        {
            services.TryAddSingleton<IConfiguration>(configuration);
            // AOT 友好的手写绑定（BindingExtensions.cs）。用 lambda 而不是 method group，
            // 这样 DatabaseList 类型变化时是编译期错误而不是运行期 InvalidCastException。
            services.Configure<DatabaseList>(list => configuration.Bind_DatabaseList(list));
            // IOptionsMonitor 依赖 change token 才会在文件变更时重算。
            // 显式注册，不要靠"再 Configure 一次"的副作用来拿 token。
            services.AddSingleton<IOptionsChangeTokenSource<DatabaseList>>(
                new ConfigurationChangeTokenSource<DatabaseList>(configuration));
            // 单例：首次构造时做初始加载并订阅变更，容器 dispose 时退订。
            services.AddSingleton<DatabaseListReloader>();
        }

        services.AddScoped(Build);
        services.AddScoped(typeof(ILightOrmRepository<>), typeof(DefaultRepository<>));
        services.AddScoped<IExpressionContext, ExpressionCoreSql>();
        return services;
    }

    private static ExpressionSqlOptions Build(IServiceProvider provider)
    {
        // 触发单例构造：初始加载 + 订阅变更都在 DatabaseListReloader 里完成。
        // 没传 configuration 时它没被注册，GetService 返回 null，行为与以前一致。
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

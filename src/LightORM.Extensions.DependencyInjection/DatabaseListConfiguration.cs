using LightORM.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;

namespace LightORM;

/// <summary>
/// 把配置节 <c>LightOrmDatabases</c> 绑定到 <see cref="DatabaseList"/>。
/// <para>
/// 这个实例在 <c>AddLightOrm</c> 时登记，而 <see cref="IConfiguration"/> 在<b>执行绑定时</b>才从容器取，
/// 所以调用顺序无关紧要（<c>services.AddSingleton&lt;IConfiguration&gt;(...)</c> 放在 <c>AddLightOrm</c> 之后也生效），
/// 容器里没有 <see cref="IConfiguration"/> 就什么都不做 —— 也就是空列表。
/// </para>
/// </summary>
internal sealed class DatabaseListConfigureOptions(IServiceProvider services) : IConfigureOptions<DatabaseList>
{
    public void Configure(DatabaseList options)
    {
        if (services.GetService<IConfiguration>() is { } configuration)
        {
            configuration.Bind_DatabaseList(options);
        }
    }
}

internal sealed class ConfigurationReloadTokenSource(IServiceProvider services) : IOptionsChangeTokenSource<DatabaseList>
{
    private IConfiguration? configuration;

    public string Name => string.Empty;

    public IChangeToken GetChangeToken()
    {
        configuration ??= services.GetService<IConfiguration>()?.GetSection("LightOrmDatabases");
        return configuration?.GetReloadToken() ?? new CancellationChangeToken(CancellationToken.None);
    }
}

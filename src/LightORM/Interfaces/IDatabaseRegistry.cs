using LightORM.Options;

namespace LightORM.Interfaces;

/// <summary>
/// 可选的扩展能力：支持"从配置创建 provider"的注册。
/// <para>
/// 单独放在一个接口里而不是塞进 <see cref="IExpressionContextSetup"/>，是为了不给第三方实现增加破坏性变更；
/// 使用方请调用扩展方法 <c>IExpressionContextSetupEx.RegisterDatabase(...)</c>，它会做能力检测。
/// </para>
/// </summary>
public interface IDatabaseRegistry
{
    /// <summary>
    /// 登记某种方言的 provider 工厂，<paramref name="type"/> 的 <see cref="DbBaseType.Name"/> 用于匹配配置文件里的 <c>Type</c>（大小写不敏感）。
    /// </summary>
    void RegisterDatabase(DbBaseType type, Func<DatabaseOptions, IDatabaseProvider> create);
}

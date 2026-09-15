using System.Data.Common;

namespace LightORM.Interfaces
{
    /// <summary>
    /// [可选能力接口] 方言在创建 DbCommand 后、绑定参数之前，对命令做专属初始化（如 Oracle 的 BindByName / LONGFetchSize）。
    /// 该特化仅 Oracle 等少数方言需要，故按 ISP 从 <see cref="IDatabaseAdapter"/> 中剥离。
    /// 框架在准备命令时经能力探测（<c>adapter is IDbCommandInitializer</c>）按需调用；
    /// 不具备该能力的适配器可完全不实现，不影响其它方言路径。
    /// </summary>
    public interface IDbCommandInitializer
    {
        void DbCommandInit(DbCommand dbCommand);
    }
}

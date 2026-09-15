using System.Data;

namespace LightORM.Interfaces;

/// <summary>
/// 数据库方言的参数绑定接口(可选实现)。
/// <para>
/// 通用参数绑定(DbParameterReader)只知道 CLR 值, 无法表达数据库特有的类型语义
/// (如 PostgreSQL 的 jsonb、xml、数组、时间戳精度等)。实现本接口的数据库适配器可在绑定阶段
/// 依据 <see cref="ITableColumnInfo"/> 与值, 施加驱动特有的类型设置。
/// </para>
/// </summary>
public interface IDatabaseParameterBinder
{
    /// <summary>
    /// 尝试接管参数的类型与值设置。
    /// </summary>
    /// <param name="parameter">由 DbCommand.CreateParameter() 创建、已设置 ParameterName 的参数, 可强转为当前驱动的具体参数类型。</param>
    /// <param name="column">参数对应的列元数据; Where 等无列参数为 null。</param>
    /// <param name="value">参数值(可能为 null)。</param>
    /// <returns>
    /// true 表示方言已完全接管(需自行设置参数类型与 Value);
    /// false 表示未接管, 由框架按 CLR 类型做默认推断(原逻辑兜底)。
    /// </returns>
    bool BindParameter(IDataParameter parameter, ITableColumnInfo? column, object? value);
}

using LightORM.Interfaces;

namespace LightORM.Models;

/// <summary>
/// SqlBuilder.DbParameters 字典中携带的结构化参数值。
/// <para>
/// <see cref="Column"/> 表示该参数对应的列元数据(可空): 写入 Insert/Update/Delete 列值时一般都有,
/// 而 Where 表达式/常量等参数通常没有(传 null)。参数绑定阶段把 <see cref="Column"/> 交给数据库方言
/// (实现 <see cref="IDatabaseParameterBinder"/> 的适配器), 使其能针对列语义施加驱动特有的类型设置
/// (如 PostgreSQL json 列设为 NpgsqlDbType.Jsonb); 没有列信息或方言未接管时, 回退到 CLR 类型默认推断。
/// </para>
/// </summary>
public readonly record struct DbParameterValue(ITableColumnInfo? Column, object? Value)
{
    /// <summary>
    /// 便捷构造"无列元数据"的参数(Column = null), 用于 Where/常量等不需要列语义的参数。
    /// 需要携带列语义(供方言类型化, 如 pg 的 jsonb)时请使用 <c>new DbParameterValue(column, value)</c>。
    /// </summary>
    public static DbParameterValue NoColumn(object? value) => new(null, value);

    public override string ToString() => Value?.ToString() ?? "NULL";
}

using System.Text;

namespace LightORM.Models;

public readonly record struct JsonColumnContext(ITableColumnInfo Column
    , IExpressionResolver Resolver
    , Stack<MemberPathInfo> Members
    , TableInfo Table)
{
    public StringBuilder Sql => Resolver.Sql;
    public ResolveContext Context => Resolver.Context;
    public SqlResolveOptions Options => Resolver.Options;

}

public enum ActionType
{
    /// <summary>
    /// 处理参数化，PgSql添加数据类型转换(::JSONB)
    /// </summary>
    Parameterized,
    /// <summary>
    /// 处理参数值，PgSql要将值转成json对象
    /// <para>
    /// 'string' => '"string"'
    /// </para>
    /// </summary>
    ParameterValue
}

public readonly record struct MapEntry(string Column, string Value);

internal readonly record struct UpsertContext(SqlBuilder Builder, Dictionary<ITableColumnInfo, MapEntry> ColumnValueMap, Dictionary<string, object> Parameters, bool IgnoreWhenMap);

internal readonly record struct BatchActionContext(SqlBuilder Builder, ITableColumnInfo[] InsertColumns, List<BatchSqlInfo> Batchs);

public readonly record struct JsonColumnParameterContext(ActionType ActionType
    , ITableColumnInfo Column
    , StringBuilder? Sql = null
    , Dictionary<ITableColumnInfo, MapEntry>? ColumnValueMap = null
    , Dictionary<string, object>? Parameters = null
    , ILightJsonHelper? JsonHelper = null
    , object? Value = null)
{
    public void UpdateMapEntry(Func<MapEntry, MapEntry> handle)
    {
        if (ColumnValueMap?.TryGetValue(Column, out var old) == true)
        {
            var n = handle(old);
            ColumnValueMap[Column] = n;
        }
    }
}

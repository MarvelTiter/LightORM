using System;
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

    public bool HasIndexInfo()
    {
        return Members.Count > 0 || Members.Any(i => i.IndexValue.HasValue);
    }
}

/// <summary>
/// [历史兼容] 已由 <see cref="IDatabaseParameterBinder"/> 取代。
/// </summary>
[Obsolete("已由 IDatabaseParameterBinder 取代, 请改为实现 IDatabaseParameterBinder.")]
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

internal readonly record struct SelectContext(SelectBuilder Builder
    , StringBuilder Sql
    , IDatabaseAdapter ScopedAdapter
    , string Ident);

internal readonly record struct UpsertContext(SqlBuilder Builder, StringBuilder Sql, Dictionary<ITableColumnInfo, MapEntry> ColumnValueMap, Dictionary<string, DbParameterValue> Parameters, bool IgnoreWhenMap, IDatabaseAdapter ScopedAdapter);

internal readonly record struct BatchActionContext(SqlBuilder Builder
    , ITableColumnInfo[] TargetColumns
    , List<BatchSqlInfo> Batchs
    , Dictionary<string, DbParameterValue> Parameters
    , IDatabaseAdapter ScopedAdapter);

internal readonly record struct BatchActionContext<TBuilder>(TBuilder Builder
    , ITableColumnInfo[] TargetColumns
    , List<BatchSqlInfo> Batchs
    , Dictionary<string, DbParameterValue> Parameters
    , IDatabaseAdapter ScopedAdapter) where TBuilder : SqlBuilder;

/// <summary>
/// [历史兼容] <see cref="IDatabaseAdapter.HandleJsonParameter"/> 的入参类型, 随其一并弃用。
/// </summary>
[Obsolete("已由 IDatabaseParameterBinder 取代, 请改为实现 IDatabaseParameterBinder.")]
public readonly record struct JsonColumnParameterContext(ActionType ActionType
    , ITableColumnInfo Column
    , StringBuilder? Sql = null
    , Dictionary<ITableColumnInfo, MapEntry>? ColumnValueMap = null
    , Dictionary<string, DbParameterValue>? Parameters = null
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

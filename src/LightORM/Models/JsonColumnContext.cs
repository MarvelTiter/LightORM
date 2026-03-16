using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

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
    /// 处理参数值，PgSql要讲值转成json对象
    /// <para>
    /// 'string' => '"string"'
    /// </para>
    /// </summary>
    ParameterValue
}

public readonly record struct JsonColumnParameterContext(ActionType ActionType
    , ITableColumnInfo? Column = null
    , StringBuilder? Sql = null
    , Dictionary<string, object>? Parameters = null
    , ILightJsonHelper? JsonHelper = null
    , object? Value = null)
{

}

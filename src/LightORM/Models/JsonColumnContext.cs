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

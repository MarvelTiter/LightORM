using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace LightORM.Models;

public readonly record struct JsonColumnContext(StringBuilder Sql
    , ITableColumnInfo Column
    , Stack<MemberPathInfo> Members
    , ResolveContext Context
    , SqlResolveOptions Options
    , TableInfo Table);

global using LightORM.Builder;
global using LightORM.Cache;
global using LightORM.ExpressionSql;
global using LightORM.Interfaces;
global using LightORM.Interfaces.ExpSql;
global using LightORM.Models;
global using LightORM.Providers.Select;
global using LightORM.SqlExecutor;
global using LightORM.Utils;
global using System;
global using System.Collections.Generic;
global using System.Data;
global using System.Diagnostics.CodeAnalysis;
global using System.Linq;
global using System.Linq.Expressions;
global using System.Threading.Tasks;
using System.Runtime.CompilerServices;
[assembly: InternalsVisibleTo("TestProject1")]


namespace System.Runtime.CompilerServices
{
    internal static class IsExternalInit { }
}

//TODO SqlExecutor对象管理优化
//TODO 开窗函数 row number, lag, join(mysql GROUP_CONCAT, oracle listagg, ..)
//TODO SelectToUpdate
//TODO SelectToInsert
//TODO IDatabaseProvider代替DbConnectInfo?
//TODO CaseWhen语句和三元表达式解析
//TODO FromQuery子查询
//TODO 添加CancellationToken参数
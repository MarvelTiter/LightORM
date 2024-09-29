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

//TODO SqlExecutor对象管理优化
//TODO 开窗函数
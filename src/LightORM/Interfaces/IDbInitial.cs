using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LightORM.DbStruct;
using LightORM.ExpressionSql;

namespace LightORM;

public interface IDbInitial
{
    IDbInitial CreateTable<T>(params T[]? datas);
    IDbInitial Configuration(Action<TableGenerateOption> option);
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LightORM.DbStruct;
using LightORM.ExpressionSql;

namespace LightORM;

public interface IDbInitial
{
    IDbInitial CreateTable<T>(string key = ConstString.Main, params T[]? datas);
    IDbInitial Configuration(Action<TableGenerateOption> option);
    string GenerateCreateSql<T>(string key = ConstString.Main);
}

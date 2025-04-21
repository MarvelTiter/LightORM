using System;
using System.Collections.Generic;
using System.Data.Common;
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

public interface IDbOption
{
    string? DbKey { get; set; }
    string? MasterConnectionString { get; set; }
    string[]? SalveConnectionStrings { get; set; }
    ISqlMethodResolver MethodResolver { get; }
    void OverrideDbProviderFactory(DbProviderFactory factory);
}

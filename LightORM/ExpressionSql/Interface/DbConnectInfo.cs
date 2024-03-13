using LightORM.Context;
using System;
using System.Data;

namespace LightORM.ExpressionSql.Interface;

internal struct DbConnectInfo
{
    public Func<IDbConnection> CreateConnection { get; set; }
    public DbBaseType DbBaseType { get; set; }
}

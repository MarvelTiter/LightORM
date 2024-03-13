using LightORM.Context;
using System;
using System.Collections.Generic;
using System.Text;

namespace LightORM.ExpressionSql.DbHandle;

internal static class DbHandleFactory
{
    public static IDbHelper GetDbHelper(this DbBaseType self)
    {
        switch (self)
        {
            case DbBaseType.SqlServer:
                return new SqlServerDb();
            case DbBaseType.SqlServer2012:
                return new SqlServerDbOver2012();
            case DbBaseType.Oracle:
                return new OracleDb();
            case DbBaseType.MySql:
                return new MySqlDb();
            case DbBaseType.Sqlite:
                return new SqliteDb();
            default:
                throw new ArgumentException();
        }
    }
}

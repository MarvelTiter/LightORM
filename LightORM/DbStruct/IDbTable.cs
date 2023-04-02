using MDbContext.ExpressionSql.DbHandle;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace MDbContext.DbStruct
{
    internal static class DbTypeEx
    {
        internal static IDbTable GetDbTable(this DbBaseType dbBaseType)
        {
            switch (dbBaseType)
            {
                case DbBaseType.SqlServer:
                    return new SqlServerDbTable();
                case DbBaseType.SqlServer2012:
                    return new SqlServerDbTable();
                case DbBaseType.Oracle:
                    return new OracleDbTable();
                case DbBaseType.MySql:
                    return new MySqlDbTable();
                case DbBaseType.Sqlite:
                    return new SqliteDbTable();
                default:
                    throw new ArgumentException();
            }
        }
    }
    internal interface IDbTable
    {
        bool GenerateDbTable<T>(IDbConnection connection, out string message);
        void SaveDbTableStruct();
    }
}

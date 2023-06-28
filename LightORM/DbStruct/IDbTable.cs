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
        internal static IDbTable GetDbTable(this DbBaseType dbBaseType, TableGenerateOption option)
        {
            switch (dbBaseType)
            {
                case DbBaseType.SqlServer:
                    return new SqlServerDbTable(option);
                case DbBaseType.SqlServer2012:
                    return new SqlServerDbTable(option);
                case DbBaseType.Oracle:
                    return new OracleDbTable(option);
                case DbBaseType.MySql:
                    return new MySqlDbTable(option);
                case DbBaseType.Sqlite:
                    return new SqliteDbTable(option);
                default:
                    throw new ArgumentException();
            }
        }
    }
    internal interface IDbTable
    {
        string GenerateDbTable<T>();
        void SaveDbTableStruct();
    }
}

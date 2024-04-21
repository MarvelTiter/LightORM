using LightORM;

namespace DatabaseUtils.Services
{
    public class DbFactory
    {
        public static IDbOperator GetDbOperator(DbBaseType databaseType, string connectionString)
        {
            return databaseType switch
            {
                DbBaseType.Oracle => new OracleDb(connectionString),
                DbBaseType.SqlServer => new SqlServerDb(connectionString),
                DbBaseType.MySql => new MySqlDb(connectionString),
                _ => throw new NotSupportedException(),
            };
        }
    }
}

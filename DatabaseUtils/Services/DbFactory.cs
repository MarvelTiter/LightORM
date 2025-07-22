using LightORM;

namespace DatabaseUtils.Services
{
    public class DbFactory
    {
        public static IDbOperator GetDbOperator(IExpressionContext context, DbBaseType databaseType, string connectionString)
        {
            return databaseType.Name switch
            {
                "Oracle" => new OracleDb(context, connectionString),
                "SqlServer" => new SqlServerDb(context, connectionString),
                "MySql" => new MySqlDb(context, connectionString),
                "PostgreSQL" => new PostgreSqlDb(context, connectionString),
                _ => throw new NotSupportedException(),
            };
        }
    }
}

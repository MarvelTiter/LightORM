using LightORM;

namespace DatabaseUtils.Services
{
    public class DbFactory
    {
        public static IDbOperator GetDbOperator(IExpressionContext context, DbBaseType databaseType, string connectionString)
        {
            return databaseType switch
            {
                DbBaseType.Oracle => new OracleDb(context, connectionString),
                DbBaseType.SqlServer => new SqlServerDb(context, connectionString),
                DbBaseType.MySql => new MySqlDb(context, connectionString),
                _ => throw new NotSupportedException(),
            };
        }
    }
}

using DatabaseUtils.Models;
using LightORM;
using LightORM.Utils;
using System.Text.RegularExpressions;

namespace DatabaseUtils.Services
{
    public class MySqlDb : DbOperatorBase, IDbOperator
    {
        private readonly string database;

        public MySqlDb(string connStr) : base(connStr)
        {
            var match = Regex.Match(connStr, @"(?<=Database\=)([A-Z|a-z]+)", RegexOptions.IgnoreCase);
            if (!match.Success)
            {
                throw new Exception("未能在连接字符串中发现目标数据库!");
            }
            database = match.Value;
        }
        public Task<IList<DatabaseTable>> GetTablesAsync()
        {
            var sql = @$"
SELECT
A.TABLE_NAME TableName
FROM INFORMATION_SCHEMA.TABLES A
WHERE a.TABLE_SCHEMA = '{database}'
";
            return Db.QueryAsync<DatabaseTable>(sql, null);
        }

        public Task<IList<TableColumn>> GetTableStructAsync(string table)
        {
            string sql = $@"
SELECT
A.COLUMN_NAME ColumnName,
A.DATA_TYPE DataType,
A.IS_Nullable Nullable,
A.COLUMN_COMMENT Comments
FROM INFORMATION_SCHEMA.COLUMNS A
WHERE A.TABLE_SCHEMA='{database}'
AND A.TABLE_NAME = '{table}'
ORDER BY A.TABLE_SCHEMA,A.TABLE_NAME,A.ORDINAL_POSITION
";
            return Db.QueryAsync<TableColumn>(sql, null);
        }

        protected override ISqlExecutor CreateDbContext()
        {
            DbConnectHelper.TryAddConnectionInfo(ConnectionString, DbBaseType.MySql, ConnectionString, MySqlConnector.MySqlConnectorFactory.Instance);
            return SqlExecutorProvider.GetExecutor(ConnectionString);
        }
    }
}

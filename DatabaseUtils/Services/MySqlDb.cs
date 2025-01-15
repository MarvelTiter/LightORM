using DatabaseUtils.Models;
using LightORM;
using LightORM.Interfaces;
using LightORM.Providers.MySql;
using LightORM.Utils;
using System.Text.RegularExpressions;

namespace DatabaseUtils.Services
{
    public class MySqlDb : DbOperatorBase, IDbOperator
    {
        private readonly string database;
        public MySqlDb(IExpressionContext context, string connStr) : base(context, connStr)
        {
            var match = Regex.Match(connStr, @"(?<=Database\=)([A-Z|a-z|_]+)", RegexOptions.IgnoreCase);
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
WHERE A.TABLE_SCHEMA = '{database}'
";
            return context.Use(GetConnectInfo()).Ado.QueryAsync<DatabaseTable>(sql, null);
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
            return context.Use(GetConnectInfo()).Ado.QueryAsync<TableColumn>(sql, null);
        }

        protected override IDatabaseProvider GetConnectInfo()
        {
            return MySqlProvider.Create(ConnectionString);
        }
    }
}

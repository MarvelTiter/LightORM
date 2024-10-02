
using DatabaseUtils.Models;
using LightORM;
using LightORM.Interfaces;
using LightORM.Providers.Oracle;
using LightORM.Utils;
using System.Text;

namespace DatabaseUtils.Services
{
    public class OracleDb : DbOperatorBase, IDbOperator
    {
        public OracleDb(IExpressionContext context, string connStr) : base(context, connStr)
        {

        }
        public async Task<IList<DatabaseTable>> GetTablesAsync()
        {
            string sql = "select table_name TableName from user_tab_columns group by table_name order by table_name";
            return await context.Use(GetConnectInfo()).Ado.QueryAsync<DatabaseTable>(sql, null);
        }

        public async Task<IList<TableColumn>> GetTableStructAsync(string table)
        {
            string sql = $@"
SELECT b.comments as Comments, 
a.column_name as ColumnName, 
a.data_type || '(' || a.data_length || ')' as DataType, 
a.data_length as Length, 
a.nullable as Nullable
FROM user_tab_columns a, user_col_comments b
WHERE a.TABLE_NAME = '{table}'
and b.table_name = '{table}'
and a.column_name = b.column_name
";
            return await context.Use(GetConnectInfo()).Ado.QueryAsync<TableColumn>(sql.ToString(), null);
        }


        protected override IDatabaseProvider GetConnectInfo()
        {
            return OracleProvider.Create(ConnectionString);
        }
    }
}


using DatabaseUtils.Models;
using LightORM;
using LightORM.Interfaces;
using LightORM.Providers.SqlServer;
using LightORM.Utils;

namespace DatabaseUtils.Services
{
    public class SqlServerDb : DbOperatorBase, IDbOperator
    {
        public SqlServerDb(IExpressionContext context, string connStr) : base(context, connStr)
        {

        }
        public async Task<IList<DatabaseTable>> GetTablesAsync()
        {
            string sql = "SELECT NAME FROM SYSOBJECTS WHERE XTYPE = 'U' ORDER BY NAME";
            return await context.Use(GetConnectInfo()).Ado.QueryListAsync<DatabaseTable>(sql);
        }

        public async Task<IList<TableColumn>> GetTableStructAsync(string table)
        {
            string sql = $@"
SELECT 
--表名 =case when a.colorder = 1 then d.name else '' end, 
--表说明 =case when a.colorder = 1 then isnull(f.value,'') else '' end, 
--字段序号 = a.colorder, 
ColumnName = a.name, 
--标识 =case when COLUMNPROPERTY(a.id, a.name,'IsIdentity')= 1 then '√'else '' end, 
--主键 =case when exists(SELECT 1 FROM sysobjects where xtype = 'PK' and name in ( 
--SELECT name FROM sysindexes WHERE indid in( 
--SELECT indid FROM sysindexkeys WHERE id = a.id AND colid = a.colid 
--  ))) then '√' else '' end, 
DataType = b.name, 
--占用字节数 = a.length, 
--长度 = COLUMNPROPERTY(a.id, a.name, 'PRECISION'), 
--小数位数 = isnull(COLUMNPROPERTY(a.id, a.name, 'Scale'), 0), 
Nullable =case when a.isnullable = 1 then '√'else '' end, 
--默认值 = isnull(e.text, ''), 
Comments = isnull(g.[value], '') 
FROM syscolumns a 
left join systypes b on a.xtype = b.xusertype 
inner join sysobjects d on a.id = d.id and d.xtype = 'U' and d.name <> 'dtproperties' 
left join syscomments e on a.cdefault = e.id 
left join sys.extended_properties g on a.id = g.major_id and a.colid = g.minor_id 
left join sys.extended_properties f on d.id = f.major_id and f.minor_id = 0 
where d.name = '{table}'--如果只查询指定表,加上此条件 
order by a.id,a.colorder 
";
            return await context.Use(GetConnectInfo()).Ado.QueryListAsync<TableColumn>(sql);

        }

        protected override IDatabaseProvider GetConnectInfo()
        {
            return SqlServerProvider.Create(SqlServerVersion.V1, ConnectionString);
        }
    }
}

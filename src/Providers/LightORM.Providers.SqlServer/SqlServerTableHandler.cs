using LightORM;
using LightORM.DbStruct;
using LightORM.Implements;
using LightORM.Providers.SqlServer.TableStructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LightORM.Providers.SqlServer;

public sealed class SqlServerTableHandler
    : BaseDatabaseHandler<SqlServerTableWriter>
{
   
    public override string GetTablesSql()
    {
        return "SELECT NAME TableName FROM SYSOBJECTS WHERE XTYPE = 'U' ORDER BY NAME";
    }

    public override string GetTableStructSql(string table)
    {
        return $"""
SELECT 
--表名 =case when a.colorder = 1 then d.name else '' end, 
--表说明 =case when a.colorder = 1 then isnull(f.value,'') else '' end, 
--字段序号 = a.colorder, 
ColumnName = a.name, 
IsIdentity =case when COLUMNPROPERTY(a.id, a.name,'IsIdentity')= 1 then 'YES' else 'NO' end, 
IsPrimaryKey =case when exists(SELECT 1 FROM sysobjects where xtype = 'PK' and name in ( 
SELECT name FROM sysindexes WHERE indid in( 
SELECT indid FROM sysindexkeys WHERE id = a.id AND colid = a.colid 
  ))) then 'YES' else 'NO' end, 
DataType = b.name, 
--占用字节数 = a.length, 
Length = COLUMNPROPERTY(a.id, a.name, 'PRECISION'), 
--小数位数 = isnull(COLUMNPROPERTY(a.id, a.name, 'Scale'), 0), 
Nullable =case when a.isnullable = 1 then 'YES'else 'NO' end, 
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
""";
    }

    /// <summary>
    /// AI 生成的代码, 用于解析 SQL Server 数据库中的数据类型到 C# 类型
    /// </summary>
    /// <param name="column"></param>
    /// <param name="type"></param>
    /// <returns></returns>
    public override bool ParseDataType(ReadedTableColumn column, out string type)
    {
         var dbType = column.DataType;
        var nullable = column.Nullable;
        if (string.IsNullOrWhiteSpace(dbType))
        {
            type = "";
            return false;
        }
        var d = dbType!.ToLower().Trim();
        var isNullable = nullable?.ToUpper() == "YES";

        switch (d)
        {
            // 字符串类型
            case "nvarchar":
            case "varchar":
            case "nchar":
            case "char":
            case "text":
            case "ntext":
            case "xml":
            case "sql_variant":
                type = "string"; // 字符串在C#中本身就是可空的
                return true;

            // 整数类型
            case "tinyint":
                type = isNullable ? "byte?" : "byte";
                break;
            case "smallint":
                type = isNullable ? "short?" : "short";
                break;
            case "int":
                type = isNullable ? "int?" : "int";
                break;
            case "bigint":
                type = isNullable ? "long?" : "long";
                break;

            // 布尔类型
            case "bit":
                type = isNullable ? "bool?" : "bool";
                break;

            // 浮点/小数类型
            case "float":
                type = isNullable ? "double?" : "double";
                break;
            case "real":
                type = isNullable ? "float?" : "float";
                break;
            case "decimal":
            case "numeric":
            case "money":
            case "smallmoney":
                type = isNullable ? "decimal?" : "decimal";
                break;

            // 日期时间类型
            case "date":
            case "datetime":
            case "smalldatetime":
            case "datetime2":
                type = isNullable ? "DateTime?" : "DateTime";
                break;
            case "datetimeoffset":
                type = isNullable ? "DateTimeOffset?" : "DateTimeOffset";
                break;
            case "time":
                type = isNullable ? "TimeSpan?" : "TimeSpan";
                break;

            // 二进制类型
            case "binary":
            case "varbinary":
            case "image":
            case "timestamp":
            case "rowversion":
                type = "byte[]";
                break;

            // GUID类型
            case "uniqueidentifier":
                type = isNullable ? "Guid?" : "Guid";
                break;

            // 不支持的类型
            default:
                type = "";
                return false;
        }

        return true;
    }
}

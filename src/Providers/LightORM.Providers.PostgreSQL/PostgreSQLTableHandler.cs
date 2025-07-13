using LightORM.DbStruct;
using LightORM.Implements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LightORM.Providers.PostgreSQL;

public sealed class PostgreSQLTableHandler(TableGenerateOption option) : BaseDatabaseHandler(option)
{
    protected override string BuildSql(DbTable table)
    {
        StringBuilder sql = new StringBuilder();

        #region Table
        var existsClause = Option.NotCreateIfExists ? " IF NOT EXISTS" : "";
        sql.Append($"""
CREATE TABLE{existsClause} {DbEmphasis(table.Name)}(
    {string.Join($",{Environment.NewLine}    ", table.Columns.Select(BuildColumn))}
)
""");

        // PostgreSQL 的表空间语法与 Oracle 不同
        if (!string.IsNullOrEmpty(Option.PostgreSQLTableSpace))
        {
            sql.Append($" TABLESPACE {Option.PostgreSQLTableSpace}");
        }
        sql.AppendLine(";");
        #endregion

        #region ColumnComment
        if (Option.SupportComment)
        {
            var comments = table.Columns.Where(col => col.Comment != null);

            foreach (var com in comments)
            {
                sql.AppendLine($"COMMENT ON COLUMN {DbEmphasis(table.Name)}.{DbEmphasis(com.Name)} IS '{com.Comment?.Replace("'", "''")}';");
            }
        }
        #endregion

        #region PrimaryKey
        var primaryKeys = table.Columns.Where(col => col.PrimaryKey);
        if (primaryKeys.Any())
        {
            // PostgreSQL 允许在列定义中直接指定主键，但这里保持与 Oracle 一致的单独约束语法
            sql.AppendLine(
$@"ALTER TABLE {DbEmphasis(table.Name)} 
ADD CONSTRAINT {GetPrimaryKeyName(table.Name, primaryKeys)} 
PRIMARY KEY ({string.Join(", ", primaryKeys.Select(item => DbEmphasis(item.Name)))});");
        }
        #endregion

        #region Index
        // 确保主键有索引（PostgreSQL 自动为主键创建索引）
        var pks = table.Columns.Where(c => c.PrimaryKey);
        foreach (var p in pks)
        {
            if (table.Indexs.Any(ind => ind.Columns.Any(s => s == p.Name) || ind.IsUnique)) continue;
            table.Indexs = table.Indexs.Concat(new DbIndex[]
            {
                new DbIndex(){
                    Columns = new string[]{p.Name },
                    DbIndexType= IndexType.Unique}
            });
        }
        int i = 1;
        foreach (DbIndex index in table.Indexs)
        {
            string columnNames = string.Join(", ", index.Columns.Select(c => $"{DbEmphasis(c)}"));
            string unique = index.IsUnique || index.DbIndexType == IndexType.Unique ? "UNIQUE " : "";

            // PostgreSQL 不支持 BITMAP 和 REVERSE 索引类型
            sql.AppendLine($"CREATE {unique}INDEX {GetIndexName(table, index, i)} ON {DbEmphasis(table.Name)}({columnNames});");
            i++;
        }

        #endregion

        return sql.ToString();
    }

    protected override string BuildColumn(DbColumn column)
    {
        string dataType = ConvertToDbType(column);

        // 处理长度限制
        if (dataType.Contains("CHAR") || dataType == "NUMERIC")
        {
            dataType = column.Length != null ? $"{dataType}({column.Length})" : dataType;
        }

        string notNull = column.NotNull || column.PrimaryKey ? " NOT NULL" : " NULL";
        string identity = column.AutoIncrement ? " GENERATED ALWAYS AS IDENTITY" : "";
        string defaultValue = column.Default != null ? $" DEFAULT {FormatDefaultValue(column.Default, dataType)}" : "";

        return $"{DbEmphasis(column.Name)} {dataType}{identity}{defaultValue}{notNull}";
    }

    protected override string ConvertToDbType(DbColumn type)
    {
        string? typeFullName;
        if (type.DataType.IsEnum)
        {
            typeFullName = Enum.GetUnderlyingType(type.DataType).FullName;
        }
        else
        {
            typeFullName = (Nullable.GetUnderlyingType(type.DataType) ?? type.DataType).FullName;
        }

        return typeFullName switch
        {
            "System.Boolean" => "BOOLEAN",
            "System.Byte" => "SMALLINT", // PostgreSQL 没有直接的 BYTE 类型
            "System.Int16" => "SMALLINT",
            "System.Int32" => "INTEGER",
            "System.Int64" => "BIGINT",
            "System.Single" => "REAL",
            "System.Double" => "DOUBLE PRECISION",
            "System.Decimal" => "NUMERIC",
            "System.DateTime" => "TIMESTAMP",
            "System.DateTimeOffset" => "TIMESTAMP WITH TIME ZONE",
            "System.Guid" => "UUID",
            "System.Byte[]" => "BYTEA",
            _ => "TEXT", // PostgreSQL 的 TEXT 类型适合任意长度字符串
        };
    }

    protected override string DbEmphasis(string name) => $"\"{name}\"";


    private string FormatDefaultValue(object value, string dataType)
    {
        if (value == null) return "NULL";

        return dataType switch
        {
            "BOOLEAN" => (bool)value ? "TRUE" : "FALSE",
            "UUID" => $"'{value}'::uuid",
            "TIMESTAMP" or "TIMESTAMP WITH TIME ZONE" => $"'{value:yyyy-MM-dd HH:mm:ss}'::timestamp",
            _ => $"'{value.ToString()?.Replace("'", "''")}'"
        };
    }
}

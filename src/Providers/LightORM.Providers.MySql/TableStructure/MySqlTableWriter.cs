using LightORM.DbStruct;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LightORM.Providers.MySql.TableStructure;

public class MySqlTableWriter : LightORM.Implements.WriteTableFromType
{
    public override IEnumerable<string> BuildTableSql(TableGenerateOption option, DbTable table)
    {
        StringBuilder sql = new StringBuilder();
        var primaryKeys = table.Columns.Where(col => col.PrimaryKey);
        string primaryKeyConstraint = "";

        if (primaryKeys.Count() > 0)
        {
            primaryKeyConstraint =
                $@",{Environment.NewLine}    CONSTRAINT {GetPrimaryKeyName(table.Name, primaryKeys)} PRIMARY KEY({string.Join(", ", primaryKeys.Select(item => $"{DbEmphasis(option, item.Name)}"))})";
        }

        var existsClause = option.NotCreateIfExists ? " IF NOT EXISTS " : "";
        sql.AppendLine(@$"
CREATE TABLE{existsClause} {DbEmphasis(option, table.Name)}(
    {string.Join($",{Environment.NewLine}    ", table.Columns.Select(col => BuildColumn(option, col)))}{primaryKeyConstraint}
)
");
        int i = 1;
        foreach (DbIndex index in table.Indexs)
        {
            string columnNames = string.Join(",", index.Columns.Select(item => $"{DbEmphasis(option, item)}"));
            string type = "";
            if (index.DbIndexType == IndexType.Unique)
            {
                type = "UNIQUE ";
            }

            sql.AppendLine($"CREATE {type}INDEX {GetIndexName(table, index, i)} ON {DbEmphasis(option, table.Name)}({columnNames});");
            i++;
        }

        yield return sql.ToString();
    }

    protected override string BuildColumn(TableGenerateOption option, DbColumn column)
    {
        string dataType = ConvertToDbType(option, column);
        if (dataType.Contains("VARCHAR"))
        {
            dataType = $"{dataType}({column.Length ?? option.DefaultStringLength})";
        }

        string notNull = column.NotNull || column.AutoIncrement || column.PrimaryKey ? "NOT NULL" : "NULL";
        string identity = column.AutoIncrement ? $"AUTO_INCREMENT" : "";
        string commentClause = !string.IsNullOrEmpty(column.Comment) && option.SupportComment ? $"COMMENT '{column.Comment}'" : "";
        string defaultValueClause = column.Default != null ? $" DEFAULT '{column.Default}'" : "";
        return $"{DbEmphasis(option, column.Name)} {dataType} {notNull} {identity} {commentClause} {defaultValueClause}";
    }

    protected override string ConvertToDbType(TableGenerateOption option, DbColumn type)
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
            "System.Boolean" => "BIT",
            "System.Byte" => "TINYINT",
            "System.Int16" => "SMALLINT",
            "System.Int32" => "INT",
            "System.Int64" => "BIGINT",
            "System.Single" => "FLOAT",
            "System.Double" => "DOUBLE",
            "System.Decimal" => "NUMERIC",
            "System.DateTime" => "DATETIME",
            "System.DateTimeOffset" => "DATETIMEOFFSET",
            "System.Guid" => "GUID",
            "System.Byte[]" => "BINARY",
            "System.Object" => "VARIANT",
            _ => "VARCHAR",
        };
    }

    protected override string DbEmphasis(TableGenerateOption option, string name) => $"`{name}`";

    private static object CheckDefaultValue(DbColumn column)
    {
        var defaultValueStr = column.Default!.ToString();
        if (defaultValueStr is not null)
        {
            if (column.DataType == typeof(bool) && column.Default is bool bVal)
            {
                return bVal ? 1 : 0;
            }
        }

        return defaultValueStr!;
    }
}
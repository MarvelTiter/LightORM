using LightORM.DbStruct;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LightORM.Providers.SqlServer.TableStructure;

public class SqlServerTableWriter : LightORM.Implements.WriteTableFromType
{
    public override IEnumerable<string> BuildTableSql(TableGenerateOption option, DbTable table)
    {
        StringBuilder sql = new StringBuilder();

        #region PrimaryKey

        var primaryKeys = table.Columns.Where(col => col.PrimaryKey).ToArray();
        string primaryKeyConstraint = "";
        if (primaryKeys.Length > 0)
        {
            primaryKeyConstraint =
                $"""
                 ,{Environment.NewLine}    CONSTRAINT {GetPrimaryKeyName(table.Name, primaryKeys)} PRIMARY KEY({string.Join($", ", primaryKeys.Select(item => $"{DbEmphasis(option, item.Name)}"))})
                 """;
        }

        #endregion

        #region Table

        string existsClause = option.NotCreateIfExists ? $"IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name='{table.Name}')" : "";
        sql.Append(
            $"""
             SET ANSI_NULLS ON
             SET QUOTED_IDENTIFIER ON
             {existsClause}
             CREATE TABLE {DbEmphasis(option, table.Name)}(
                 {string.Join($",{Environment.NewLine}    ", table.Columns.Select(col => BuildColumn(option, col)))}{primaryKeyConstraint}
             );

             """);

        #endregion

        #region ColumnConment

        if (option.SupportComment)
        {
            var comments = table.Columns.Where(col => col.Comment != null);

            foreach (var com in comments)
            {
                sql.AppendLine($"EXEC sp_addextendedproperty N'MS_Description',N'{com.Comment}',N'SCHEMA',N'dbo',N'table',N'{table.Name}',N'COLUMN',N'{com.Name}'");
            }
        }

        #endregion

        #region Default

        var defaults = table.Columns.Where(col => col.Default != null);
        foreach (var def in defaults)
        {
            var defaultValue = CheckDefaultValue(def);
            sql.AppendLine($"ALTER TABLE {DbEmphasis(option, table.Name)} ADD CONSTRAINT DF_{table.Name}_{def.Name}  DEFAULT '{defaultValue}' FOR {DbEmphasis(option, def.Name)}");
        }

        #endregion

        #region Index

        int i = 1;
        foreach (DbIndex index in table.Indexs)
        {
            string columnNames = string.Join(",", index.Columns);
            string unique = index.IsUnique ? "UNIQUE " : "";
            string clustered = index.IsClustered ? "CLUSTERED " : "NONCLUSTERED ";
            string type = index.DbIndexType == IndexType.ColumnStore ? "COLUMNSTORE " : "";
            sql.AppendLine($@"CREATE {unique}{clustered}{type}INDEX {GetIndexName(table, index, i)} ON {DbEmphasis(option, table.Name)}({columnNames})");
            i++;
        }

        #endregion

        yield return sql.ToString();
    }

    protected override string BuildColumn(TableGenerateOption option, DbColumn column)
    {
        var dbType = ConvertToDbType(option, column);
        string dataType = $"{DbEmphasis(option, column.Name)} {dbType}{(dbType.ToUpper().Contains("CHAR") ? $"({column.Length ?? option.DefaultStringLength})" : "")}";
        string identity = column.AutoIncrement ? " IDENTITY(1,1)" : "";
        string notNull = column.NotNull ? " NOT NULL" : "";
        return $"{dataType}{identity}{notNull}";
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
            "System.Boolean" => "Bit",
            "System.Byte" => "TinyInt",
            "System.Int16" => "SmallInt",
            "System.Int32" => "Int",
            "System.Int64" => "BigInt",
            "System.Single" => "Numeric",
            "System.Double" => "Float",
            "System.Decimal" => "Money",
            "System.DateTime" => "DateTime",
            "System.DateTimeOffset" => "DateTimeOffset",
            "System.Guid" => "UniqueIdentifier",
            "System.Byte[]" => "Binary",
            "System.Object" => "Variant",
            _ => option.UseUnicodeString ? "NVarChar" : "VarChar",
        };
    }

    protected override string DbEmphasis(TableGenerateOption option, string name) => $"[{name}]";

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
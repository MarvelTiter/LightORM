using LightORM.DbStruct;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LightORM.Providers.Sqlite.TableStructure;

public class SqliteTableWriter : LightORM.Implements.WriteTableFromType
{
    private readonly ThreadLocal<bool> useAutoIncrement = new ThreadLocal<bool>(() => false);

    public override IEnumerable<string> BuildTableSql(TableGenerateOption option, DbTable table)
    {
        StringBuilder sql = new StringBuilder();
        DbColumn[] primaryKeys = [.. table.Columns.Where(col => col.PrimaryKey)];
        string primaryKeyConstraint = "";
        useAutoIncrement.Value = table.Columns.Any(c => c.AutoIncrement);

        if (primaryKeys.Length > 0 && !useAutoIncrement.Value)
        {
            primaryKeyConstraint =
                $"""
                 ,{Environment.NewLine}    PRIMARY KEY ({string.Join($", ", primaryKeys.Select(item => $"{DbEmphasis(option, item.Name)}"))})
                 """;
        }

        var existsClause = option.NotCreateIfExists ? " IF NOT EXISTS " : "";
        sql.AppendLine($"""
                        CREATE TABLE{existsClause} {DbEmphasis(option, table.Name)}(
                            {string.Join($",{Environment.NewLine}    ", table.Columns.Select(c => BuildColumn(option, c)))}{primaryKeyConstraint}
                        );
                        """);
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
        string identity = column.AutoIncrement ? $"AUTOINCREMENT" : "";
        string notNull = column.NotNull || column.PrimaryKey ? "NOT NULL" : "NULL";
        if (column.AutoIncrement)
        {
            notNull = "";
            dataType = $"{dataType} PRIMARY KEY";
        }
        else if (column.PrimaryKey && useAutoIncrement.Value)
        {
            notNull = "NOT NULL UNIQUE";
        }

        // string commentClause = !string.IsNullOrEmpty(column.Comment) && Option.SupportComment ? $"COMMENT '{column.Comment}'" : "";
        string defaultValueClause = column.Default != null ? $" DEFAULT '{column.Default}'" : "";
        return $"{DbEmphasis(option, column.Name)} {dataType} {notNull} {identity} {defaultValueClause}";
    }

    protected override string ConvertToDbType(TableGenerateOption option, DbColumn type)
    {
        if (type.DataType == typeof(byte) || type.DataType == typeof(byte?)
                                          || type.DataType == typeof(sbyte) || type.DataType == typeof(sbyte?)
                                          || type.DataType == typeof(short) || type.DataType == typeof(short?)
                                          || type.DataType == typeof(ushort) || type.DataType == typeof(ushort?)
                                          || type.DataType == typeof(int) || type.DataType == typeof(int?)
                                          || type.DataType == typeof(uint) || type.DataType == typeof(uint?)
                                          || type.DataType == typeof(long) || type.DataType == typeof(long?)
                                          || type.DataType == typeof(ulong) || type.DataType == typeof(ulong?)
                                          || type.DataType == typeof(bool) || type.DataType == typeof(bool?))
        {
            return "INTEGER";
        }
        else if (type.DataType == typeof(float) || type.DataType == typeof(float?)
                                                || type.DataType == typeof(double) || type.DataType == typeof(double?)
                                                || type.DataType == typeof(decimal) || type.DataType == typeof(decimal?))
        {
            return "REAL";
        }
        else if (type.DataType == typeof(byte[]))
        {
            return "BLOB";
        }
        else
        {
            return "TEXT";
        }
    }

    protected override string DbEmphasis(TableGenerateOption option, string name) => $"`{name}`";
}
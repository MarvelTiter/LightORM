﻿using LightORM.DbStruct;
using LightORM.Implements;
using System.Text;

namespace LightORM.Providers.Sqlite;

public sealed class SqliteTableHandler(TableGenerateOption option) : BaseDatabaseHandler(option)
{
    protected override string BuildSql(DbTable table)
    {
        StringBuilder sql = new StringBuilder();
        var primaryKeys = table.Columns.Where(col => col.PrimaryKey);
        string primaryKeyConstraint = "";

        if (primaryKeys.Count() > 0)
        {
            primaryKeyConstraint =
$@"
,CONSTRAINT {GetPrimaryKeyName(primaryKeys)} PRIMARY KEY
(
{string.Join($",{Environment.NewLine}", primaryKeys.Select(item => $"{DbEmphasis(item.Name)}"))}
)";
        }

        var existsClause = Option.NotCreateIfExists ? " IF NOT EXISTS " : "";
        sql.AppendLine(@$"
CREATE TABLE{existsClause} {DbEmphasis(table.Name)}(
{string.Join($",{Environment.NewLine}", table.Columns.Select(col => BuildColumn(col)))}
{primaryKeyConstraint}
);
");
        int i = 1;
        foreach (DbIndex index in table.Indexs)
        {
            string columnNames = string.Join(",", index.Columns.Select(item => $"{DbEmphasis(item)}"));
            string type = "";
            if (index.DbIndexType == IndexType.Unique)
            {
                type = "UNIQUE ";
            }
            sql.AppendLine($"CREATE {type}INDEX {GetIndexName(table, index, i)} ON {DbEmphasis(table.Name)}({columnNames});");
            i++;
        }

        return sql.ToString();
    }

    protected override string BuildColumn(DbColumn column)
    {
        string dataType = ConvertToDbType(column);
        string notNull = column.NotNull ? "NOT NULL" : "NULL";
        string identity = column.AutoIncrement ? $"AUTO_INCREMENT" : "";
        string commentClause = !string.IsNullOrEmpty(column.Comment) && Option.SupportComment ? $"COMMENT '{column.Comment}'" : "";
        string defaultValueClause = column.Default != null ? $" DEFAULT '{column.Default}'" : "";
        return $"{DbEmphasis(column.Name)} {dataType} {notNull} {identity} {commentClause} {defaultValueClause}";
    }

    /// https://learn.microsoft.com/zh-cn/dotnet/standard/data/sqlite/types
    protected override string ConvertToDbType(DbColumn type)
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

    protected override string DbEmphasis(string name) => $"`{name}`";
}

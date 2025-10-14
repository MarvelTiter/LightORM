using LightORM.DbStruct;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LightORM.Providers.Dameng.TableStructure;

public class DamengTableWriter : LightORM.Implements.WriteTableFromType
{
    public override IEnumerable<string> BuildTableSql(TableGenerateOption option, DbTable table)
    {
        // TODO 达梦 TableSpace
        var tableSpace = option?.OracleTableSpace != null ? $"TABLESPACE {option.OracleTableSpace}" : "";

        #region Table

        yield return $"""
                      CREATE TABLE {DbEmphasis(option, table.Name)}(
                          {string.Join($",{Environment.NewLine}    ", table.Columns.Select(c => BuildColumn(option, c)))}
                      ){tableSpace}

                      """;

        #endregion

        #region ColumnConment

        if (option!.SupportComment)
        {
            var comments = table.Columns.Where(col => col.Comment != null);

            foreach (var com in comments)
            {
                yield return $"COMMENT ON COLUMN {AttachUserId(option, table.Name)}.{DbEmphasis(option, com.Name)} IS '{com.Comment}'";
            }
        }

        #endregion

        #region Index

        {
            var pks = table.Columns.Where(c => c.PrimaryKey);
            var it = pks.Count() > 1 ? IndexType.Normal : IndexType.Unique;
            foreach (var p in pks)
            {
                if (table.Indexs.Any(ind => ind.Columns.Any(s => s == p.Name) || ind.IsUnique)) continue;
                table.Indexs = table.Indexs.Concat(
                [
                    new() { Columns = [p.Name], DbIndexType = it}
                ]);
            }
            //if (pks.Count() == 1)
            //{
            //    var pkColumn = pks.First();
            //    bool existsUniqueIndex = table.Indexs.Any(index =>
            //        index.Columns.Count() == 1 &&
            //        index.Columns.First() == pkColumn.Name &&
            //        (index.IsUnique || index.DbIndexType == IndexType.Unique));
            //    if (!existsUniqueIndex)
            //    {
            //        table.Indexs = table.Indexs.Concat(
            //    [
            //        new() { Columns = [pkColumn.Name], DbIndexType = IndexType.Unique }
            //    ]);
            //    }
            //}
        }

        int i = 1;
        foreach (DbIndex index in table.Indexs)
        {
            string columnNames = string.Join(",", index.Columns.Select(c => $"{DbEmphasis(option, c)}"));
            var type = "";
            if (index.IsUnique || index.DbIndexType == IndexType.Unique)
            {
                type = "UNIQUE ";
            }

            //else if (index.DbIndexType == IndexType.Bitmap)
            //{
            //    type = "BITMAP ";
            //}
            string reverse = index.DbIndexType == IndexType.Reverse ? "REVERSE" : "";
            yield return $"CREATE {type}INDEX {DbEmphasis(option, CheckIdxLength(table, index, i))} ON {DbEmphasis(option, table.Name)}({columnNames}){reverse}";
            i++;
        }

        #endregion

        #region PrimaryKey

        var primaryKeys = table.Columns.Where(col => col.PrimaryKey);
        if (primaryKeys.Any())
        {
            yield return
                $"""
                 ALTER TABLE {AttachUserId(option, table.Name)} ADD CONSTRAINT {CheckPkLength(table.Name, primaryKeys)} PRIMARY KEY
                 (
                     {string.Join($",{Environment.NewLine}    ", primaryKeys.Select(item => $"{DbEmphasis(option, item.Name)}"))}
                 )
                 """
                ;
        }

        #endregion
    }

    protected override string BuildColumn(TableGenerateOption option, DbColumn column)
    {
        string dataType = ConvertToDbType(option, column);
        if (dataType.Contains("VARCHAR"))
        {
            dataType = $"{dataType}({column.Length ?? option.DefaultStringLength})";
        }

        string notNull = column.NotNull || column.PrimaryKey ? "NOT NULL" : "NULL";
        string identity = column.AutoIncrement ? $"IDENTITY(1, 1)" : "";
        string defaultValueClause = column.Default != null ? $" DEFAULT '{column.Default}'" : "";
        return $"{DbEmphasis(option, column.Name)} {dataType} {defaultValueClause} {notNull} {identity}";
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
            "System.Boolean" => "CHAR(1)",
            "System.Byte" => "TINYINT",
            "System.Int16" => "SMALLINT",
            "System.Int32" => "INT",
            "System.Int64" => "BIGINT",
            "System.Single" => "FLOAT",
            "System.Double" => "DOUBLE",
            "System.Decimal" => "DECIMAL(33,3)",
            "System.DateTime" => "DATE",
            //"System.DateTimeOffset" => "DateTimeOffset",
            "System.Guid" => "CHAR(36)",
            "System.Byte[]" => "BLOB",
            //"System.Object" => "Variant",
            _ => option.UseUnicodeString ? "NVARCHAR2" : "VARCHAR2",
        };
    }

    protected override string DbEmphasis(TableGenerateOption option, string name) => $"\"{name.ToUpper()}\"";

    private static string CheckPkLength(string name, IEnumerable<DbColumn> pks)
    {
        var originKey = GetPrimaryKeyName(name, pks);
        if (originKey.Length < 30)
        {
            return originKey;
        }

        var over = originKey.Length - 30;
        var parts = pks.Count() + 1;
        var splitCount = (over / parts) + 1;
        return $"PK_{name.Substring(splitCount)}_{string.Join("_", pks.Select(c => c.Name.Substring(splitCount)))}";
    }

    private static string CheckIdxLength(DbTable info, DbIndex index, int i)
    {
        var originKey = GetIndexName(info, index, i);
        if (originKey.Length < 30)
        {
            return originKey;
        }

        var over = originKey.Length - 30;
        var parts = index.Columns.Count() + 1;
        var splitCount = (over / parts) + 1;
        return $"IDX_{info.Name?.Substring(splitCount)}_{string.Join("_", index.Columns.Select(c => c.Substring(splitCount)))}_{i}";
    }

    private string AttachUserId(TableGenerateOption option, string name)
    {
        if (option.OracleUserId != null)
            return $"\"{option.OracleUserId}\".\"{name.ToUpper()}\"";
        else
            return DbEmphasis(option, name);
    }
}
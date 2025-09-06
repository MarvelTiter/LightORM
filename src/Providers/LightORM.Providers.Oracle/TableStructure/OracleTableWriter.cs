using LightORM.DbStruct;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LightORM.Providers.Oracle.TableStructure;

public class OracleTableWriter : LightORM.Implements.WriteTableFromType
{
    public override IEnumerable<string> BuildTableSql(TableGenerateOption option, DbTable table)
    {
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
            foreach (var p in pks)
            {
                if (table.Indexs.Any(ind => ind.Columns.Any(s => s == p.Name) || ind.IsUnique)) continue;
                table.Indexs = table.Indexs.Concat(
                [
                    new() { Columns = new string[] { p.Name }, DbIndexType = IndexType.Unique }
                ]);
            }
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
            else if (index.DbIndexType == IndexType.Bitmap)
            {
                type = "BITMAP ";
            }

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

        if (!option.OracleOverVersion)
        {
            // 序列 + 触发器自增
            var increments = table.Columns.Where(col => col.AutoIncrement);
            foreach (var col in increments)
            {
                var triName = AttachUserId(option, $"TRI_{table.Name}_{col.Name}").ToUpper();
                var seqName = $"SEQ_{table.Name}_{col.Name}".ToUpper();
                yield return $"""
                              CREATE SEQUENCE {AttachUserId(option, seqName)} START WITH 1 INCREMENT BY 1MINVALUE 1 MAXVALUE 999999999999999 ORDER
                              """;
                yield return $"""
                              CREATE OR REPLACE TRIGGER {triName}
                                  BEFORE INSERT ON {DbEmphasis(option, table.Name.ToUpper())}
                                  FOR EACH ROW
                              BEGIN
                                  IF :NEW.{DbEmphasis(option, col.Name.ToUpper())} IS NULL THEN
                                      SELECT {DbEmphasis(option, seqName)}.NEXTVAL INTO :NEW.{DbEmphasis(option, col.Name.ToUpper())} FROM DUAL;
                                  END IF;
                              END;
                              """;

                yield return $"ALTER TRIGGER {triName} ENABLE";
            }
        }
    }

    protected override string BuildColumn(TableGenerateOption option, DbColumn column)
    {
        string dataType = ConvertToDbType(option, column);
        if (dataType.Contains("VARCHAR"))
        {
            dataType = $"{dataType}({column.Length ?? option.DefaultStringLength})";
        }

        string notNull = column.NotNull || column.PrimaryKey ? "NOT NULL" : "NULL";
        string identity = column.AutoIncrement && option.OracleOverVersion ? $"GENERATED ALWAYS AS IDENTITY" : "";
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
            "System.Byte" => "NUMBER(3)",
            "System.Int16" => "NUMBER(5)",
            "System.Int32" => "NUMBER(10)",
            "System.Int64" => "NUMBER(19)",
            "System.Single" => "NUMBER(7,3)",
            "System.Double" => "NUMBER(15,5)",
            "System.Decimal" => "DECIMAL(33,3)",
            "System.DateTime" => "DATE",
            //"System.DateTimeOffset" => "DateTimeOffset",
            "System.Guid" => "RAW(16)",
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
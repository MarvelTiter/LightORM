using LightORM.DbStruct;
using LightORM.Implements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LightORM.Providers.Oracle;

public sealed class OracleTableHandler(TableGenerateOption option) : BaseDatabaseHandler(option)
{
    protected override string BuildSql(DbTable table)
    {
        StringBuilder sql = new StringBuilder();

        var tableSpace = Option?.OracleTableSpace != null ? $"TABLESPACE {Option.OracleTableSpace}" : "";

        #region Table
        sql.Append($"""
CREATE TABLE {DbEmphasis(table.Name)}(
    {string.Join($",{Environment.NewLine}    ", table.Columns.Select(BuildColumn))}
){tableSpace}

""");
        #endregion

        #region ColumnConment
        if (Option!.SupportComment)
        {
            var comments = table.Columns.Where(col => col.Comment != null);

            foreach (var com in comments)
            {
                sql.AppendLine($"COMMENT ON COLUMN {AttachUserId(table.Name)}.{DbEmphasis(com.Name)} IS '{com.Comment}';");
            }
        }
        #endregion

        #region Index

        {
            var pks = table.Columns.Where(c => c.PrimaryKey);
            foreach (var p in pks)
            {
                if (table.Indexs.Any(ind => ind.Columns.Any(s => s == p.Name) || ind.IsUnique)) continue;
                table.Indexs = table.Indexs.Concat(new DbIndex[]
                {
                    new DbIndex(){ Columns = new string[]{p.Name }, DbIndexType= IndexType.Unique }
                });
            }
        }

        int i = 1;
        foreach (DbIndex index in table.Indexs)
        {
            string columnNames = string.Join(",", index.Columns.Select(c => $"{DbEmphasis(c)}"));
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
            sql.AppendLine($"CREATE {type}INDEX {GetIndexName(table, index, i)} ON {DbEmphasis(table.Name)}({columnNames}){reverse}");
            i++;
        }

        #endregion

        #region PrimaryKey
        var primaryKeys = table.Columns.Where(col => col.PrimaryKey);
        if (primaryKeys.Count() > 0)
        {
            sql.AppendLine(
$"""
ALTER TABLE {AttachUserId(table.Name)} ADD CONSTRAINT {GetPrimaryKeyName(table.Name, primaryKeys)} PRIMARY KEY
(
    {string.Join($",{Environment.NewLine}    ", primaryKeys.Select(item => $"{DbEmphasis(item.Name)}"))}
)
"""
);
        }
        #endregion

        if (!Option.OracleOverVersion)
        {
            // 序列 + 触发器自增
            var increments = table.Columns.Where(col => col.AutoIncrement);
            foreach (var col in increments)
            {
                var triName = AttachUserId($"TRI_{table.Name}_{col.Name}").ToUpper();
                var autoIncrement = $"""
 CREATE SEQUENCE {AttachUserId($"SEQ_{table.Name}_{col.Name}").ToUpper()} START WITH 1 INCREMENT BY 1 MINVALUE 1 MAXVALUE 999999999999999 ORDER
 CREATE OR REPLACE TRIGGER {triName}
     BEFORE INSERT ON {DbEmphasis(table.Name.ToUpper())}
     FOR EACH ROW
 BEGIN
     IF :NEW.{DbEmphasis(col.Name.ToUpper())} IS NULL THEN
         SELECT SEQ_{table.Name.ToUpper()}_{col.Name.ToUpper()}.NEXTVAL INTO :NEW.{DbEmphasis(col.Name.ToUpper())} FROM DUAL
     END IF
 END
 ALTER TRIGGER {triName} ENABLE
 """;
                sql.AppendLine(autoIncrement);
            }
        }

        return sql.ToString();
    }

    protected override string BuildColumn(DbColumn column)
    {
        string dataType = ConvertToDbType(column);
        if (dataType.Contains("VARCHAR"))
        {
            dataType = $"{dataType}({column.Length ?? Option.DefaultStringLength})";
        }
        string notNull = column.NotNull || column.PrimaryKey ? "NOT NULL" : "NULL";
        string identity = column.AutoIncrement && Option.OracleOverVersion ? $"GENERATED ALWAYS AS IDENTITY" : "";
        string defaultValueClause = column.Default != null ? $" DEFAULT '{column.Default}'" : "";
        return $"{DbEmphasis(column.Name)} {dataType} {defaultValueClause} {notNull} {identity}";
    }

    /// https://www.cnblogs.com/liufuhuang/articles/3020009.html
    protected override string ConvertToDbType(DbColumn type)
    {
        string? typeFullName = "";
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
            _ => Option.UseUnicodeString ? "NVARCHAR2" : "VARCHAR2",
        };
    }

    protected override string DbEmphasis(string name) => $"\"{name.ToUpper()}\"";
    private string AttachUserId(string name)
    {
        if (Option.OracleUserId != null)
            return $"\"{Option.OracleUserId}\".\"{name.ToUpper()}\"";
        else
            return DbEmphasis(name);
    }
}

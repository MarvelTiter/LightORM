using LightORM;
using LightORM.DbStruct;

namespace LightORM.Providers.Oracle;

partial class OracleTableHandler
{
    public override IEnumerable<string> BuildTableSql(OracleTableOptions option, DbTable table)
    {
        var tableSpace = option.TableSpace != null ? $"TABLESPACE {option.TableSpace}" : "";

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
            var it = pks.Count() > 1 ? IndexType.Normal : IndexType.Unique;
            foreach (var p in pks)
            {
                if (table.Indexs.Any(ind => ind.Columns.Any(s => s == p.Name) || ind.IsUnique)) continue;
                table.Indexs = table.Indexs.Concat(
                [
                    new() { Columns = [p.Name], DbIndexType = it }
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

        if (!UseIdentityColumn)
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

    protected override string BuildColumn(OracleTableOptions option, DbColumn column)
    {
        string dataType = ConvertToDbType(option, column);
        if (dataType.Contains("VARCHAR"))
        {
            dataType = $"{dataType}({column.Length ?? option.DefaultStringLength})";
        }

        // Oracle 的身份列有两条硬约束（实测 21c）：
        // ① GENERATED ... AS IDENTITY 必须紧跟数据类型，写在 NOT NULL 之后报 ORA-00907；
        // ② 身份列隐含 NOT NULL，显式写 NULL 报 ORA-30670 —— 故两者必须一起决定。
        bool useIdentity = column.AutoIncrement && UseIdentityColumn;
        string identity = useIdentity ? " GENERATED ALWAYS AS IDENTITY" : "";
        string notNull = useIdentity || column.NotNull || column.PrimaryKey ? "NOT NULL" : "NULL";
        string defaultValueClause = column.Default != null ? $" DEFAULT '{column.Default}'" : "";
        return $"{DbEmphasis(option, column.Name)} {dataType}{identity} {defaultValueClause} {notNull}";
    }

    protected override string ConvertToDbType(OracleTableOptions option, DbColumn type)
    {
        if (type.IsJson && option.JSONBackend != Models.JSONBackend.NotSupport)
        {
            if (option.SpecificJsonColumnDbType is not null)
            {
                return option.SpecificJsonColumnDbType;
            }
            // 超长文档必须走大对象；原生 JSON 类型要 21c 起才有（≤19c 建表报 ORA-00902 invalid datatype）。
            // 退化成 CLOB 不损失功能：JSON_VALUE / JSON_QUERY / JSON_TRANSFORM 对文本列同样有效。
            if (type.Length > 320000 || !UseJsonNativeType)
            {
                return "CLOB";
            }
            return "JSON";
        }
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

    

}
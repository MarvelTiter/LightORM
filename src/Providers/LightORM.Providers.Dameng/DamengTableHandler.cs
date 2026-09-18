using LightORM.DbStruct;
using LightORM.Implements;

namespace LightORM.Providers.Dameng;

public sealed partial class DamengTableHandler(DamengTableOptions tableOptions)
    : BaseDatabaseHandler<DamengTableOptions>
{
    public override DamengTableOptions Options => tableOptions;

    /// <summary>
    /// 列出当前用户（schema）下的表。达梦不跨 schema 自动搜索，<c>USER_TABLES</c> 就是当前连接的 schema。
    /// </summary>
    public override string GetTablesSql()
    {
        return """
               SELECT TABLE_NAME AS TableName
               FROM USER_TABLES
               ORDER BY TABLE_NAME
               """;
    }

    public override string GetTableStructSql(string table)
    {
        var t = EscapeLiteral(table);
        return $"""
                SELECT
                    tc.COLUMN_NAME                                                           AS ColumnName,
                    CASE WHEN tc.DATA_TYPE = 'BLOB' AND s.SCALE = 16384
                         THEN 'JSON' ELSE tc.DATA_TYPE END                                   AS DataType,
                    CASE
                        WHEN s.TYPE$ IN ('TEXT','CLOB','BLOB','IMAGE','LONGVARCHAR','LONGVARBINARY') THEN ''
                        WHEN s.TYPE$ IN ('DECIMAL','NUMERIC','NUMBER','DEC') AND s.LENGTH$ > 0 THEN
                             TO_CHAR(s.LENGTH$) ||
                             CASE WHEN s.SCALE > 0 AND s.SCALE < 1000
                                  THEN ',' || TO_CHAR(s.SCALE) ELSE '' END
                        WHEN s.LENGTH$ > 0 THEN TO_CHAR(s.LENGTH$)
                        ELSE ''
                    END                                                                      AS Length,
                    CASE WHEN tc.NULLABLE = 'Y' THEN 'YES' ELSE 'NO' END                     AS Nullable,
                    NVL(tc.DATA_DEFAULT, '')                                                 AS DefaultValue,
                    CASE WHEN EXISTS (
                             SELECT 1
                             FROM USER_CONS_COLUMNS cc
                             JOIN USER_CONSTRAINTS c ON cc.CONSTRAINT_NAME = c.CONSTRAINT_NAME
                             WHERE c.CONSTRAINT_TYPE = 'P'
                               AND cc.TABLE_NAME = tc.TABLE_NAME
                               AND cc.COLUMN_NAME = tc.COLUMN_NAME
                         ) THEN 'YES' ELSE 'NO' END                                      AS IsPrimaryKey,
                    CASE WHEN (NVL(s.INFO2, 0) & 1) = 1 THEN 'YES' ELSE 'NO' END             AS IsIdentity,
                    NVL(cm.COMMENTS, '')                                                     AS Comments
                FROM USER_TAB_COLUMNS tc
                LEFT JOIN USER_COL_COMMENTS cm
                       ON cm.TABLE_NAME = tc.TABLE_NAME AND cm.COLUMN_NAME = tc.COLUMN_NAME
                LEFT JOIN SYSOBJECTS o
                       ON o.NAME = tc.TABLE_NAME AND o.TYPE$ = 'SCHOBJ' AND o.SUBTYPE$ = 'UTAB'
                LEFT JOIN SYSCOLUMNS s
                       ON s.ID = o.ID AND s.NAME = tc.COLUMN_NAME
                WHERE tc.TABLE_NAME = '{t}'
                ORDER BY tc.COLUMN_ID
                """;
    }


    public override bool ParseDataType(ReadedTableColumn column, out string type)
    {
        var dbType = column.DataType;
        if (string.IsNullOrWhiteSpace(dbType))
        {
            type = "";
            return false;
        }

        var declared = dbType!;
        var paren = declared.IndexOf('(');
        if (paren >= 0)
        {
            // 不用 range 语法（[..paren]）：net462 上没有 System.Range / System.Index。
            declared = declared.Substring(0, paren);
        }

        var d = declared.Trim().ToUpperInvariant();
        var isNullable = column.Nullable?.ToUpperInvariant() is "YES" or "Y" or "TRUE" or "1";

        switch (d)
        {
            // ---- 整数族 ----
            case "TINYINT":
            case "BYTE":
                type = isNullable ? "sbyte?" : "sbyte";
                return true;
            case "SMALLINT":
            case "INT2":
                type = isNullable ? "short?" : "short";
                return true;
            case "MEDIUMINT":
            case "INT":
            case "INTEGER":
            case "INT4":
                type = isNullable ? "int?" : "int";
                return true;
            case "BIGINT":
            case "INT8":
                type = isNullable ? "long?" : "long";
                return true;

            // ---- 位 ----
            case "BIT":
                type = isNullable ? "bool?" : "bool";
                return true;

            // ---- 字符（含达梦把 NCLOB / XMLTYPE 归一化成的 TEXT）----
            case "CHAR":
            case "CHARACTER":
            case "NCHAR":
            case "VARCHAR":
            case "VARCHAR2":
            case "NVARCHAR":
            case "NVARCHAR2":
            case "TEXT":
            case "CLOB":
            case "NCLOB":
            case "LONGVARCHAR":
            case "XMLTYPE":
            // ---- JSON：GetTableStructSql 已把达梦的 BLOB 存储改写成类型名 JSON ----
            case "JSON":
            case "JSONB":
                type = isNullable ? "string?" : "string";
                return true;

            // ---- 定点：精度/标度在 Length 里（"18,2" 或 "9"），空表示没写精度 ----
            case "DECIMAL":
            case "NUMERIC":
            case "NUMBER":
            case "DEC":
                type = NumericType(column, isNullable);
                return true;

            // ---- 浮点（达梦 FLOAT 是 8 字节双精度，单精度是 REAL）----
            case "FLOAT":
            case "DOUBLE":
            case "DOUBLE PRECISION":
            case "BINARY_DOUBLE":
                type = isNullable ? "double?" : "double";
                return true;
            case "REAL":
            case "BINARY_FLOAT":
                type = isNullable ? "float?" : "float";
                return true;

            // ---- 日期时间（达梦 DATE 带时分秒，等价 DateTime）----
            case "DATE":
            case "DATETIME":
            case "TIMESTAMP":
                type = isNullable ? "DateTime?" : "DateTime";
                return true;
            case "TIME":
                type = isNullable ? "TimeOnly?" : "TimeOnly";
                return true;

            // ---- 二进制 ----
            case "BLOB":
            case "BINARY":
            case "VARBINARY":
            case "IMAGE":
            case "LONGVARBINARY":
            case "RAW":
            case "LONG RAW":
                type = "byte[]";
                return true;

            default:
                type = "";
                return false;
        }
    }

    /// <summary>
    /// 定点数的精度细分：带标度（<c>"18,2"</c>）一律 <c>decimal</c>；只有精度时按
    /// 4 / 9 / 18 位分档到 <c>short</c> / <c>int</c> / <c>long</c>；
    /// 达梦的裸 <c>NUMBER</c>（无参）是浮点语义，给 <c>decimal</c> 容错 —— 给 <c>int</c> 遇到小数会直接失败。
    /// </summary>
    private static string NumericType(ReadedTableColumn column, bool isNullable)
    {
        var len = column.Length;
        if (!string.IsNullOrWhiteSpace(len) && int.TryParse(len!.Split(',')[0].Trim(), out var precision) && precision > 0)
        {
            if (len.Contains(',')) return isNullable ? "decimal?" : "decimal";
            if (precision <= 4) return isNullable ? "short?" : "short";
            if (precision <= 9) return isNullable ? "int?" : "int";
            if (precision <= 18) return isNullable ? "long?" : "long";
        }
        return isNullable ? "decimal?" : "decimal";
    }

    /// <summary>表名要放进字符串字面量，单引号必须转义成两个单引号。</summary>
    private static string EscapeLiteral(string value) => value.Replace("'", "''");
}

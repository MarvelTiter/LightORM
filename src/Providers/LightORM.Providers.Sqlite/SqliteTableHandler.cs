using LightORM.DbStruct;
using LightORM.Implements;

namespace LightORM.Providers.Sqlite;

public sealed partial class SqliteTableHandler(SqliteTableOptions generateOption, SqliteCapabilities capabilities)
    : BaseDatabaseHandler<SqliteTableOptions>
{
    /// <summary>版本能力档案，决定 JSON 列走 jsonb 还是 json 族。</summary>
    public SqliteCapabilities Capabilities => capabilities;

    public override SqliteTableOptions Options => generateOption;

    /// <summary>
    /// 列出当前库的用户表。
    /// <para>
    /// SQLite 没有 schema 概念，表信息在 <c>sqlite_master</c>；<c>sqlite_%</c> 是引擎自身的表
    /// （<c>sqlite_sequence</c> 等），必须排除。
    /// </para>
    /// </summary>
    public override string GetTablesSql()
    {
        return """
               SELECT name AS TableName
               FROM sqlite_master
               WHERE type = 'table'
                 AND name NOT LIKE 'sqlite\_%' ESCAPE '\'
               ORDER BY name
               """;
    }

    public override string GetTableStructSql(string table)
    {
        var t = EscapeLiteral(table);
        return $"""
                SELECT
                    ti.name                                     AS ColumnName,
                    ti.type                                     AS DataType,
                    CASE WHEN ti."notnull" = 1 OR ti.pk > 0 THEN 'NO' ELSE 'YES' END AS Nullable,
                    ''                                          AS Comments,
                    COALESCE(ti.dflt_value, '')                 AS DefaultValue,
                    CASE WHEN ti.pk > 0 THEN 'YES' ELSE 'NO' END AS IsPrimaryKey,
                    CASE WHEN instr(ti.type, '(') > 0
                         THEN replace(substr(ti.type, instr(ti.type, '(') + 1), ')', '')
                         ELSE '' END                            AS Length,
                    CASE WHEN ti.pk > 0
                              AND upper(trim(ti.type)) = 'INTEGER'
                              AND (SELECT COUNT(*) FROM pragma_table_info('{t}') p WHERE p.pk > 0) = 1
                         THEN 'YES' ELSE 'NO' END               AS IsIdentity
                FROM pragma_table_info('{t}') ti
                ORDER BY ti.cid
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
            case "UNSIGNED BIG INT":
                type = isNullable ? "ulong?" : "ulong";
                return true;

            // ---- 布尔（SQLite 无布尔类型，按惯例用 INTEGER 存 0/1）----
            case "BOOLEAN":
            case "BOOL":
                type = isNullable ? "bool?" : "bool";
                return true;

            // ---- 浮点 / 定点 ----
            case "REAL":
            case "DOUBLE":
            case "DOUBLE PRECISION":
                type = isNullable ? "double?" : "double";
                return true;
            case "FLOAT":
                type = isNullable ? "float?" : "float";
                return true;
            case "NUMERIC":
            case "DECIMAL":
                type = isNullable ? "decimal?" : "decimal";
                return true;

            // ---- 文本 ----
            case "TEXT":
            case "CLOB":
            case "CHAR":
            case "CHARACTER":
            case "VARCHAR":
            case "NCHAR":
            case "NVARCHAR":
            case "VARYING CHARACTER":
            case "NATIVE CHARACTER":
            case "JSON":
                type = isNullable ? "string?" : "string";
                return true;

            // ---- 日期时间（SQLite 无专用类型，按声明名约定）----
            case "DATE":
                type = isNullable ? "DateOnly?" : "DateOnly";
                return true;
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
                type = "byte[]";
                return true;

            // ---- 其它常见别名 ----
            case "UUID":
            case "GUID":
            case "UNIQUEIDENTIFIER":
                type = isNullable ? "Guid?" : "Guid";
                return true;

            // 未命中精确表 → 按亲和性关键字兜底（顺序遵循 SQLite 官方规则）
            default:
                if (d.Contains("INT"))
                {
                    type = isNullable ? "int?" : "int";
                    return true;
                }
                if (d.Contains("CHAR") || d.Contains("CLOB") || d.Contains("TEXT"))
                {
                    type = isNullable ? "string?" : "string";
                    return true;
                }
                if (d.Contains("BLOB"))
                {
                    type = "byte[]";
                    return true;
                }
                if (d.Contains("REAL") || d.Contains("FLOA") || d.Contains("DOUB"))
                {
                    type = isNullable ? "double?" : "double";
                    return true;
                }
                type = isNullable ? "object?" : "object";
                return false;
        }
    }

    /// <summary>
    /// 表名要放进字符串字面量（<c>pragma_table_info('...')</c>），单引号必须转义成两个单引号，
    /// 否则名字里带引号的表会把查询拆坏。
    /// </summary>
    private static string EscapeLiteral(string value) => value.Replace("'", "''");
}

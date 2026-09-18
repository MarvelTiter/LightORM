using LightORM.DbStruct;
using LightORM.Implements;

namespace LightORM.Providers.Oracle;

public sealed partial class OracleTableHandler(OracleTableOptions tableOptions, OracleCapabilities capabilities)
    : BaseDatabaseHandler<OracleTableOptions>
{
    public override OracleTableOptions Options => tableOptions;

    /// <summary>
    /// 自增列是否用 <c>GENERATED ALWAYS AS IDENTITY</c>（12.1+）。
    /// 版本来自 <see cref="OracleCapabilities"/>（探测或 <see cref="TableOptions.SpecificVersion"/> 显式指定）；
    /// 版本未知时为 false，退化为序列 + 触发器 —— 与旧 <c>OverVersion=false</c> 的默认行为一致。
    /// </summary>
    private bool UseIdentityColumn => capabilities.Features.HasFlag(OracleFeatures.IdentityColumn);

    /// <summary>
    /// JSON 列是否用原生 <c>JSON</c> 类型（21c+）。关掉时退化 <c>CLOB</c> 文本列（≤19c 唯一可行写法）。
    /// 版本未知时为 false —— 与"探测不到版本就退到全版本可用的写法"一致。
    /// </summary>
    private bool UseJsonNativeType => capabilities.Features.HasFlag(OracleFeatures.JsonNativeType);

    public override string GetTablesSql()
    {
        return "select table_name TableName from user_tab_columns group by table_name order by table_name";
    }

    public override IEnumerable<string> GetDropTableSql(DbTable table)
    {
        var dt = base.GetDropTableSql(table);
        foreach (var item in dt)
        {
            yield return item;
        }
        if (!UseIdentityColumn)
        {
            // 序列 + 触发器自增
            var increments = table.Columns.Where(col => col.AutoIncrement);
            foreach (var col in increments)
            {
                var seqName = $"SEQ_{table.Name}_{col.Name}".ToUpper();
                yield return $"""
                              DROP SEQUENCE {AttachUserId(tableOptions, seqName)}
                              """;
            }
        }
    }

    public override string GetTableStructSql(string table)
    {
        return $"""
                SELECT 
                    tc.column_name AS "ColumnName",
                    tc.data_type AS "DataType",
                    CASE 
                        WHEN tc.data_type IN ('VARCHAR2', 'CHAR', 'NVARCHAR2', 'NCHAR') 
                        THEN TO_CHAR(tc.char_length)
                        WHEN tc.data_type = 'NUMBER' AND tc.data_precision IS NOT NULL 
                        THEN TO_CHAR(tc.data_precision) || 
                             CASE WHEN tc.data_scale > 0 THEN ',' || TO_CHAR(tc.data_scale) ELSE '' END
                        ELSE TO_CHAR(tc.data_length)
                    END AS "Length",
                    CASE WHEN tc.nullable = 'Y' THEN 'YES' ELSE 'NO' END AS "Nullable",
                    NVL(tc.data_default,'') AS "DefaultValue",
                    CASE WHEN (SELECT '1' 
                     FROM user_cons_columns cc
                     JOIN user_constraints c ON cc.constraint_name = c.constraint_name
                     WHERE c.constraint_type = 'P'
                     AND cc.table_name = tc.table_name
                     AND cc.column_name = tc.column_name
                     AND ROWNUM = 1) = '1' THEN 'YES' ELSE 'NO' END AS "IsPrimaryKey",
                    NVL(cc.comments, '') AS "Comments"
                FROM 
                    user_tab_columns tc
                LEFT JOIN 
                    user_col_comments cc ON tc.table_name = cc.table_name 
                    AND tc.column_name = cc.column_name
                WHERE 
                    tc.table_name = UPPER('{table}')  -- 替换为您的表名
                ORDER BY 
                    tc.column_id
                """;
    }

    public override bool ParseDataType(ReadedTableColumn column, out string type)
    {
        var dbType = column.DataType;
        var nullable = column.Nullable;
        if (string.IsNullOrWhiteSpace(dbType))
        {
            type = "";
            return false;
        }

        // 处理可能带长度的类型声明（如 VARCHAR2(50)）
        var baseType = dbType!.Split('(')[0].ToUpper().Trim();
        var isNullable = nullable?.ToUpper() == "Y" || nullable?.ToUpper() == "YES";

        switch (baseType)
        {
            // 字符串类型
            case "VARCHAR2":
            case "NVARCHAR2":
            case "CHAR":
            case "NCHAR":
            case "LONG":
            case "CLOB":
            case "NCLOB":
            case "ROWID":
            case "UROWID":
                type = isNullable ? "string?" : "string"; // 字符串在C#中本身就是可空的
                return true;

            // 数值类型
            case "NUMBER":
                // Oracle NUMBER 类型需要特殊处理，可能映射到不同C#类型
                if (dbType.Contains("NUMBER("))
                {
                    // 尝试解析精度和小数位
                    var parts = dbType.Split(['(', ',', ')']);
                    if (parts.Length >= 2 && int.TryParse(parts[1], out var precision))
                    {
                        if (parts.Length >= 3 && int.TryParse(parts[2], out var scale))
                        {
                            // 有小数位的情况
                            type = isNullable ? "decimal?" : "decimal";
                            return true;
                        }

                        // 只有精度的情况
                        if (precision <= 4)
                            type = isNullable ? "short?" : "short";
                        else if (precision <= 9)
                            type = isNullable ? "int?" : "int";
                        else if (precision <= 18)
                            type = isNullable ? "long?" : "long";
                        else
                            type = isNullable ? "decimal?" : "decimal";
                        return true;
                    }
                }

                // 默认处理
                type = isNullable ? "int?" : "int";
                break;

            // 整数类型（Oracle的特定整数类型）
            case "BINARY_INTEGER":
            case "PLS_INTEGER":
                type = isNullable ? "int?" : "int";
                break;

            // 浮点类型
            case "BINARY_FLOAT":
                type = isNullable ? "float?" : "float";
                break;
            case "BINARY_DOUBLE":
            case "FLOAT":
                type = isNullable ? "double?" : "double";
                break;

            // 日期时间类型
            case "DATE":
                type = isNullable ? "DateTime?" : "DateTime";
                break;
            case "TIMESTAMP":
            case "TIMESTAMP WITH TIME ZONE":
            case "TIMESTAMP WITH LOCAL TIME ZONE":
                type = isNullable ? "DateTimeOffset?" : "DateTimeOffset";
                break;
            case "INTERVAL YEAR TO MONTH":
            case "INTERVAL DAY TO SECOND":
                type = isNullable ? "TimeSpan?" : "TimeSpan";
                break;

            // 二进制类型
            case "BLOB":
            case "BFILE":
            case "RAW":
            case "LONG RAW":
                type = "byte[]";
                break;

            // XML类型
            case "XMLTYPE":
                type = isNullable ? "string?" : "string"; // 或 System.Xml.XmlDocument/XElement
                return true;

            // 布尔类型（Oracle没有原生BOOLEAN，但有时用NUMBER(1)表示）
            case "BOOLEAN": // 某些Oracle版本支持
                type = isNullable ? "bool?" : "bool";
                break;

            // 不支持的类型
            default:
                type = "";
                return false;
        }

        return true;
    }


    private string AttachUserId(OracleTableOptions option, string name)
    {
        if (option.UserId != null)
            return $"\"{option.UserId}\".\"{name.ToUpper()}\"";
        else
            return DbEmphasis(option, name);
    }
    protected override string DbEmphasis(OracleTableOptions option, string name) => $"\"{name.ToUpper()}\"";
}
using LightORM.DbStruct;
using LightORM.Implements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LightORM.Providers.MySql.TableStructure;

namespace LightORM.Providers.MySql;

public sealed class MySqlTableHandler(string database)
    : BaseDatabaseHandler<MySqlTableWriter>
{
    public override string GetTablesSql()
    {
        return $"""
                SELECT
                A.TABLE_NAME TableName
                FROM INFORMATION_SCHEMA.TABLES A
                WHERE A.TABLE_SCHEMA = '{database}'
                """;
    }

    public override string GetTableStructSql(string table)
    {
        return $"""
                SELECT
                A.COLUMN_NAME ColumnName,
                A.DATA_TYPE DataType,
                A.IS_Nullable Nullable,
                A.COLUMN_COMMENT Comments,
                A.COLUMN_DEFAULT DefaultValue,
                IF(COLUMN_KEY = 'PRI', 'YES', 'NO') IsPrimaryKey,
                A.CHARACTER_MAXIMUM_LENGTH Length,
                IF(EXTRA = 'auto_increment', 'YES', 'NO') IsIdentity
                FROM INFORMATION_SCHEMA.COLUMNS A
                WHERE A.TABLE_SCHEMA='{database}'
                AND A.TABLE_NAME = '{table}'
                ORDER BY A.TABLE_SCHEMA,A.TABLE_NAME,A.ORDINAL_POSITION
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

        // 处理可能带长度/精度的类型声明
        var baseType = dbType.Split('(')[0].ToUpper().Trim();
        var isNullable = nullable?.ToUpper() == "YES" || nullable?.ToUpper() == "Y";

        switch (baseType)
        {
            // 字符串类型 - 即使string是引用类型，也按照可空特性要求加?
            case "CHAR":
            case "VARCHAR":
            case "TINYTEXT":
            case "TEXT":
            case "MEDIUMTEXT":
            case "LONGTEXT":
            case "ENUM":
            case "SET":
            case "JSON":
                type = isNullable ? "string?" : "string";
                return true;

            // 整数类型
            case "TINYINT":
                if (dbType.ToUpper().Contains("TINYINT(1)"))
                    type = isNullable ? "bool?" : "bool";
                else
                    type = isNullable ? "sbyte?" : "sbyte";
                break;

            case "SMALLINT":
            case "YEAR":
                type = isNullable ? "short?" : "short";
                break;

            case "MEDIUMINT":
            case "INT":
            case "INTEGER":
                type = isNullable ? "int?" : "int";
                break;

            case "BIGINT":
                type = isNullable ? "long?" : "long";
                break;

            // 位类型
            case "BIT":
                type = dbType.ToUpper().Contains("BIT(1)")
                    ? (isNullable ? "bool?" : "bool")
                    : (isNullable ? "ulong?" : "ulong");
                break;

            // 小数/浮点类型
            case "DECIMAL":
            case "NUMERIC":
                type = isNullable ? "decimal?" : "decimal";
                break;

            case "FLOAT":
                type = isNullable ? "float?" : "float";
                break;

            case "DOUBLE":
                type = isNullable ? "double?" : "double";
                break;

            // 日期时间类型
            case "DATE":
                type = isNullable ? "DateOnly?" : "DateOnly"; // 或 DateTime?
                break;

            case "DATETIME":
            case "TIMESTAMP":
                type = isNullable ? "DateTime?" : "DateTime";
                break;

            case "TIME":
                type = isNullable ? "TimeOnly?" : "TimeOnly"; // 或 TimeSpan?
                break;

            // 二进制类型
            case "BINARY":
            case "VARBINARY":
            case "TINYBLOB":
            case "BLOB":
            case "MEDIUMBLOB":
            case "LONGBLOB":
                type = "byte[]"; // 数组已经是引用类型，不需要?
                break;

            // 不支持的类型
            default:
                type = "";
                return false;
        }

        return true;
    }
}
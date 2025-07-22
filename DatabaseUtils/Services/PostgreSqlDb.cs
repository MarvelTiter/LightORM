using DatabaseUtils.Models;
using LightORM;
using LightORM.Interfaces;
using LightORM.Providers.PostgreSQL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DatabaseUtils.Services;

internal class PostgreSqlDb(IExpressionContext context, string connStr) : DbOperatorBase(context, connStr), IDbOperator
{
    public Task<IList<DatabaseTable>> GetTablesAsync()
    {
        string sql = """
SELECT 
    table_schema as SchemaName,
    table_name as TableName
FROM 
    information_schema.tables
WHERE 
    table_schema NOT IN ('pg_catalog', 'information_schema')
ORDER BY 
    table_schema, table_name
""";
        return context.Use(GetConnectInfo()).Ado.QueryListAsync<DatabaseTable>(sql, null);
    }

    public Task<IList<TableColumn>> GetTableStructAsync(string table)
    {
        string sql = $"""
            SELECT 
                c.column_name as columnname,
                c.data_type as datatype,
                c.is_nullable,
                COALESCE(c.column_default::text,'') as defaultvalue,
                c.is_identity as isidentity,
                pgd.description as comments,
                CASE WHEN c.data_type IN ('character varying', 'varchar', 'char', 'text') THEN COALESCE(c.character_maximum_length::text, '') ELSE '' END AS length,
                CASE WHEN pk.column_name IS NOT NULL THEN 'YES' ELSE 'NO' END as isprimarykey
            FROM 
                information_schema.columns c
            LEFT JOIN 
                pg_catalog.pg_statio_all_tables st ON c.table_schema = st.schemaname AND c.table_name = st.relname
            LEFT JOIN 
                pg_catalog.pg_description pgd ON pgd.objoid = st.relid AND pgd.objsubid = c.ordinal_position
            LEFT JOIN (
                SELECT 
                    kcu.column_name 
                FROM 
                    information_schema.table_constraints tc
                JOIN 
                    information_schema.key_column_usage kcu ON tc.constraint_name = kcu.constraint_name
                WHERE 
                    tc.constraint_type = 'PRIMARY KEY' 
                    AND tc.table_name = '{table}'
            ) pk ON c.column_name = pk.column_name
            WHERE 
                c.table_name = '{table}'
            ORDER BY 
                c.ordinal_position
            """;
        return context.Use(GetConnectInfo()).Ado.QueryListAsync<TableColumn>(sql, null);
    }

    public override bool ParseDataType(TableColumn column, out string type)
    {
        var dbType = column.DataType;
        var nullable = column.Nullable;
        if (string.IsNullOrWhiteSpace(dbType))
        {
            type = "";
            return false;
        }
        var d = dbType.ToLower().Trim();
        var isNullable = nullable?.ToUpper() == "YES";

        switch (dbType)
        {
            case "integer":
                type = isNullable ? "int?" : "int";
                return true;
            case "bigint":
                type = isNullable ? "long?" : "long";
                return true;
            case "smallint":
                type = isNullable ? "short?" : "short";
                return true;
            case "numeric":
            case "decimal":
                type = isNullable ? "decimal?" : "decimal";
                return true;
            case "real":
                type = isNullable ? "float?" : "float";
                return true;
            case "double precision":
                type = isNullable ? "double?" : "double";
                return true;
            case "boolean":
                type = isNullable ? "bool?" : "bool";
                return true;
            case "text":
            case "character varying":
            case "varchar":
                type = isNullable ? "string?" : "string";
                return true;
            case "timestamp without time zone":
            case "timestamp":
                type = isNullable ? "DateTime?" : "DateTime";
                return true;
            case "date":
                type = isNullable ? "DateOnly?" : "DateOnly";
                return true;
            case "time without time zone":
                type = isNullable ? "TimeOnly?" : "TimeOnly";
                return true;
            case "bytea":
                type = isNullable ? "byte[]?" : "byte[]";
                return true;
            case "uuid":
                type = isNullable ? "Guid?" : "Guid";
                return true;
            case "json":
            case "jsonb":
                type = isNullable ? "string?" : "string"; // 或者可以返回特定的JSON类型
                return true;
            default:
                type = isNullable ? "object?" : "object";
                return false;
        }
    }

    protected override IDatabaseProvider GetConnectInfo()
    {
        return PostgreSQLProvider.Create(ConnectionString);
    }
}

using MDbContext.SqlExecutor;
using System;
using System.Data;
using System.Linq;
using System.Text;

namespace MDbContext.DbStruct
{
    internal class SqlServerDbTable : DbTableBase
    {
        internal override void BuildSql(IDbConnection connection, DbTable info)
        {
            // create table
            StringBuilder sb = new StringBuilder();
            sb.Append("CREATE TABLE [").Append(info.Name).Append("] (\n");
            //foreach (var col in info.Columns)
            //{
            //    sb.Append($"{col.Name}");
            //}
            var cols = info.Columns.Select(col => GenerateColumn(col));
            sb.Append(string.Join(",\n", cols)).Append(')');
            connection.Execute(sb.ToString());
        }

        private string GenerateColumn(DbColumn col)
        {
            StringBuilder sb = new StringBuilder();
            var dbType = ConvertToDbType(col);
            sb.AppendFormat("\t[{0}] {1}", col.Name, dbType);
            if (dbType.ToUpperInvariant().Contains("CHAR"))
            {
                sb.Append($"({col.Length ?? 255})");
            }
            if (col.PrimaryKey)
            {
                sb.Append(" PRIMARY KEY");
                if (col.AutoIncrement)
                {
                    sb.Append(" IDENTITY(1,1) ");
                }
            }
            if (col.NotNull)
            {
                sb.Append(" NOT NULL");
            }
            var defaultValueStr = col.Default?.ToString();
            if (defaultValueStr is not null)
            {
                if (!col.PrimaryKey)
                {
                    if ((dbType.Contains("CHAR") || dbType.Contains("TEXT"))
                        && !defaultValueStr.StartsWith("N'") && !defaultValueStr.StartsWith("'") != true)
                    {
                        sb.AppendFormat(" DEFAULT(N'{0}')", col.Default);
                    }
                    else
                    {
                        switch (dbType)
                        {
                            case "BIT":
                                if (col.Default is bool bVal)
                                {
                                    sb.AppendFormat(" DEFAULT({0}) ", bVal ? 1 : 0);
                                }
                                break;
                            default:
                                sb.AppendFormat(" DEFAULT({0}) ", col.Default);
                                break;
                        }
                    }
                }
            }
            return sb.ToString();
        }

        internal override bool CheckTableExists(IDbConnection connection, DbTable dbTable)
        {
            return false;
        }

        internal override string ConvertToDbType(DbColumn type)
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
                "System.Boolean" => "Bit",
                "System.Byte" => "TinyInt",
                "System.Int16" => "SmallInt",
                "System.Int32" => "Int",
                "System.Int64" => "BigInt",
                "System.Single" => "Numeric",
                "System.Double" => "Float",
                "System.Decimal" => "Money",
                "System.DateTime" => "DateTime",
                "System.DateTimeOffset" => "DateTimeOffset",
                "System.Guid" => "UniqueIdentifier",
                "System.Byte[]" => "Binary",
                "System.Object" => "Variant",
                _ => "NVarChar",
            };
        }
    }
}

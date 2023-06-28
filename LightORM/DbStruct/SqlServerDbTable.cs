using MDbContext.SqlExecutor;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;

namespace MDbContext.DbStruct
{
    internal class SqlServerDbTable : DbTableBase
    {

        public SqlServerDbTable(TableGenerateOption option) : base(option)
        {

        }

        internal override string BuildSql(DbTable table)
        {
            // create table
            StringBuilder sql = new StringBuilder();

            var primaryKeys = table.Columns.Where(col => col.PrimaryKey);
            string primaryKeyConstraint = "";

            if (primaryKeys.Count() > 0)
            {
                primaryKeyConstraint =
$@"
,CONSTRAINT {GetPrimaryKeyName(primaryKeys)} PRIMARY KEY
(
{string.Join($",{Environment.NewLine}", primaryKeys.Select(item => $"{DbEmphasis(item.Name)}"))}
)";
            }

            string existsClause = Option.NotCreateIfExists ? $"IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name='{(table.Name)}')" : "";
            sql.Append($@"
SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
{existsClause}
CREATE TABLE {DbEmphasis(table.Name)}(
{string.Join($",{Environment.NewLine}", table.Columns.Select(col => BuildColumn(col)))}
{primaryKeyConstraint}
);
");

            var comments = table.Columns.Where(col => col.Comment != null);

            foreach (var com in comments)
            {
                string type = "";

                if (com.PrimaryKey)
                {
                    type = "CONSTRAINT";
                }
                else if (table.Indexs.Any(i => i.Columns.Contains(com.Name)))
                {
                    type = "INDEX";
                }
                else
                {
                    type = "COLUMN";
                }

                sql.AppendLine($"EXEC sp_addextendedproperty N'MS_Description',N'{(com.Comment)}',N'SCHEMA',N'dbo',N'table',N'{table.Name}',N'{type}',N'{com.Name}'");
            }


            return sql.ToString();
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
                _ => Option.UseUnicodeString ? "NVarChar" : "VarChar",
            };
        }

        internal override string BuildColumn(DbColumn column)
        {
            StringBuilder sb = new StringBuilder();
            var dbType = ConvertToDbType(column);
            sb.AppendFormat("\t[{0}] {1}", column.Name, dbType);
            if (dbType.ToUpperInvariant().Contains("CHAR"))
            {
                sb.Append($"({column.Length ?? Option.DefaultStringLength})");
            }
            if (column.AutoIncrement)
            {
                sb.Append(" IDENTITY(1,1) ");
            }
            if (column.NotNull)
            {
                sb.Append(" NOT NULL");
            }
            var defaultValueStr = column.Default?.ToString();
            if (defaultValueStr is not null)
            {
                if (!column.PrimaryKey)
                {
                    if ((dbType.Contains("CHAR") || dbType.Contains("TEXT"))
                        && !defaultValueStr.StartsWith("N'") && !defaultValueStr.StartsWith("'") != true)
                    {
                        sb.AppendFormat(" DEFAULT(N'{0}')", column.Default);
                    }
                    else
                    {
                        switch (dbType)
                        {
                            case "BIT":
                                if (column.Default is bool bVal)
                                {
                                    sb.AppendFormat(" DEFAULT({0}) ", bVal ? 1 : 0);
                                }
                                break;
                            default:
                                sb.AppendFormat(" DEFAULT({0}) ", column.Default);
                                break;
                        }
                    }
                }
            }
            return sb.ToString();
        }

        internal override string DbEmphasis(string name) => $"[{name}]";
    }
}

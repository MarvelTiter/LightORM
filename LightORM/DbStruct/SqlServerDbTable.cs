using System;
using System.Data;
using System.Linq;
using System.Reflection;
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
            StringBuilder sql = new StringBuilder();

            #region PrimaryKey
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
            #endregion

            #region Table
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
            #endregion

            #region ColumnConment
            if (Option.SupportComment)
            {
                var comments = table.Columns.Where(col => col.Comment != null);

                foreach (var com in comments)
                {
                    sql.AppendLine($"EXEC sp_addextendedproperty N'MS_Description',N'{(com.Comment)}',N'SCHEMA',N'dbo',N'table',N'{table.Name}',N'COLUMN',N'{com.Name}'");
                }
            }
            #endregion

            #region Default
            var defaults = table.Columns.Where(col => col.Default != null);
            foreach (var def in defaults)
            {
                var defaultValue = CheckDefaultValue(def);
                sql.AppendLine($"ALTER TABLE {DbEmphasis(table.Name)} ADD CONSTRAINT {($"DF_{table.Name}_{def.Name}")}  DEFAULT {defaultValue} FOR {DbEmphasis(def.Name)}");
            }
            #endregion

            #region Index
            int i = 1;
            foreach (DbIndex index in table.Indexs)
            {
                string columnNames = string.Join(",", index.Columns);
                string unique = index.IsUnique ? "UNIQUE " : "";
                string clustered = index.IsClustered ? "CLUSTERED " : "NONCLUSTERED ";
                string type = index.DbIndexType == IndexType.ColumnStore ? "COLUMNSTORE " : "";
                sql.AppendLine($@"CREATE {unique}{clustered}{type}INDEX {GetIndexName(table, index, i)} ON {DbEmphasis(table.Name)}({columnNames})");
                i++;
            }

            #endregion

            return sql.ToString();
        }

        private object CheckDefaultValue(DbColumn column)
        {
            var defaultValueStr = column.Default!.ToString();
            if (defaultValueStr is not null)
            {
                if (column.DataType == typeof(bool) && column.Default is bool bVal)
                {
                    return bVal ? 1 : 0;
                }
            }
            return defaultValueStr!;
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
            var dbType = ConvertToDbType(column);
            string dataType = $"{DbEmphasis(column.Name)} {dbType}{(dbType.ToUpper().Contains("CHAR") ? $"({column.Length ?? Option.DefaultStringLength})" : "")}";
            string identity = column.AutoIncrement ? " IDENTITY(1,1)" : "";
            string notNull = column.NotNull ? " NOT NULL" : "";
            return $"{dataType}{identity}{notNull}";
        }

        internal override string DbEmphasis(string name) => $"[{name}]";
    }
}

using System;
using System.Data;
using System.Linq;
using System.Text;

namespace MDbContext.DbStruct
{
    internal class MySqlDbTable : DbTableBase
    {
        public MySqlDbTable(TableGenerateOption option) : base(option)
        {

        }

        internal override string BuildColumn(DbColumn column)
        {
            string dataType = ConvertToDbType(column);
            string notNull = column.NotNull ? "NOT NULL" : "NULL";
            string identity = column.AutoIncrement ? $"AUTO_INCREMENT" : "";
            string commentClause = !string.IsNullOrEmpty(column.Comment) && Option.SupportComment ? $"COMMENT '{column.Comment}'" : "";
            string defaultValueClause = column.Default != null ? $" DEFAULT '{column.Default}'" : "";
            return $"{DbEmphasis(column.Name)} {dataType} {notNull} {identity} {commentClause} {defaultValueClause}";
        }

        internal override string BuildSql(DbTable table)
        {
            StringBuilder sql = new StringBuilder();
            var primaryKeys = table.Columns.Where(col => col.PrimaryKey);
            string primaryKeyConstraint = "";

            if (primaryKeys.Count() > 0)
            {
                primaryKeyConstraint =
$@"
,CONSTRAINT {DbTableBase.GetPrimaryKeyName(primaryKeys)} PRIMARY KEY
(
{string.Join($",{Environment.NewLine}", primaryKeys.Select(item => $"{DbEmphasis(item.Name)}"))}
)";
            }

            var existsClause = Option.NotCreateIfExists ? " IF NOT EXISTS " : "";
            sql.AppendLine(@$"
CREATE TABLE{existsClause} {DbEmphasis(table.Name)}(
{string.Join($",{Environment.NewLine}", table.Columns.Select(col => BuildColumn(col)))}
{primaryKeyConstraint}
);
");
            int i = 1;
            foreach (DbIndex index in table.Indexs)
            {
                string columnNames = string.Join(",", index.Columns.Select(item => $"{DbEmphasis(item)}"));
                string type = "";
                if (index.DbIndexType == IndexType.Unique)
                {
                    type = "UNIQUE ";
                }
                sql.AppendLine($"CREATE {type}INDEX {DbTableBase.GetIndexName(table, index, i)} ON {DbEmphasis(table.Name)}({columnNames});");
                i++;
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
                "System.Single" => "Float",
                "System.Double" => "Double",
                "System.Decimal" => "Numeric",
                "System.DateTime" => "DateTime",
                "System.DateTimeOffset" => "DateTimeOffset",
                "System.Guid" => "Guid",
                "System.Byte[]" => "Binary",
                "System.Object" => "Variant",
                _ => Option.UseUnicodeString ? "NVarChar" : "VarChar",
            };
        }

        internal override string DbEmphasis(string name) => $"`{name}`";
    }
}

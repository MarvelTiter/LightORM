using MDbContext.SqlExecutor;
using System;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;

namespace MDbContext.DbStruct
{
    internal class SqliteDbTable : DbTableBase
    {
        public SqliteDbTable(TableGenerateOption option) : base(option)
        {

        }
        internal override string BuildSql(DbTable info)
        {
            StringBuilder sql = new StringBuilder();
            var primaryKeys = info.Columns.Where(col => col.PrimaryKey);
            string primaryKeyConstraint = "";

            if (primaryKeys.Count() > 0)
            {
                primaryKeyConstraint =
$@"
,CONSTRAINT {GetPrimaryKeyName(primaryKeys)} PRIMARY KEY
 (
  {string.Join(Environment.NewLine, primaryKeys.Select(item => $"{DbEmphasis(item.Name)},")).TrimEnd(',')}
 )";
            }

            var existsClause = Option.NotCreateIfExists ? " IF NOT EXISTS " : "";
            sql.AppendLine(@$"
CREATE TABLE{existsClause} {DbEmphasis(info.Name)}(
 {string.Join($",{Environment.NewLine}", info.Columns.Select(col => BuildColumn(col)))}
 {primaryKeyConstraint}
);
");
            int i = 1;
            foreach (DbIndex index in info.Indexs)
            {
                string columnNames = string.Join(",", index.Columns.Select(item => $"{DbEmphasis(item)}"));
                string type = "";
                if (index.DbIndexType == IndexType.Unique)
                {
                    type = "UNIQUE ";
                }
                sql.AppendLine($"CREATE {type}INDEX {GetIndexName(info, index, i)} ON {DbEmphasis(info.Name)}({columnNames});");
                i++;
            }

            return sql.ToString();
        }

        internal override string BuildColumn(DbColumn column)
        {
            string dataType = ConvertToDbType(column);
            string notNull = column.NotNull ? "NOT NULL" : "NULL";
            string identity = column.AutoIncrement ? $"AUTO_INCREMENT" : "";
            string commentClause = (!string.IsNullOrEmpty(column.Comment) ? $"COMMENT '{column.Comment}'" : "");
            string defaultValueClause = column.Default != null ? $" DEFAULT {column.Default}" : "";
            return $"{DbEmphasis(column.Name)} {dataType} {notNull} {identity} {commentClause} {defaultValueClause}";
        }

        //internal override bool CheckTableExists(IDbConnection connection, DbTable dbTable)
        //{
        //    var sql = $"SELECT count(*) FROM sqlite_master WHERE type='table' AND name = '{dbTable.Name}'";
        //    var count = connection.ExecuteScale(sql);
        //    return count != null && ((int)Convert.ChangeType(count, typeof(int))) > 0;
        //}

        /// <summary>
        /// https://learn.microsoft.com/zh-cn/dotnet/standard/data/sqlite/types
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        internal override string ConvertToDbType(DbColumn type)
        {
            if (type.DataType == typeof(byte) || type.DataType == typeof(byte?)
                || type.DataType == typeof(sbyte) || type.DataType == typeof(sbyte?)
                || type.DataType == typeof(short) || type.DataType == typeof(short?)
                || type.DataType == typeof(ushort) || type.DataType == typeof(ushort?)
                || type.DataType == typeof(int) || type.DataType == typeof(int?)
                || type.DataType == typeof(uint) || type.DataType == typeof(uint?)
                || type.DataType == typeof(long) || type.DataType == typeof(long?)
                || type.DataType == typeof(ulong) || type.DataType == typeof(ulong?)
                || type.DataType == typeof(bool) || type.DataType == typeof(bool?))
            {
                return "INTEGER";
            }
            else if (type.DataType == typeof(float) || type.DataType == typeof(float?)
                || type.DataType == typeof(double) || type.DataType == typeof(double?)
                || type.DataType == typeof(decimal) || type.DataType == typeof(decimal?))
            {
                return "REAL";
            }
            else if (type.DataType == typeof(byte[]))
            {
                return "BLOB";
            }
            else
            {
                return "TEXT";
            }
        }

        internal override string DbEmphasis(string name) => $"`{name}`";
    }
}

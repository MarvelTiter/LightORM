using MDbContext.Extension;
using MDbContext.SqlExecutor;
using MDbEntity.Attributes;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace MDbContext.DbStruct
{
    internal class SqliteDbTable : DbTableBase
    {        
        internal override void BuildSql(IDbConnection connection, DbTable info)
        {
            // create table
            StringBuilder sb = new StringBuilder();
            sb.Append("CREATE TABLE ").Append(info.Name).Append("(\n");
            //foreach (var col in info.Columns)
            //{
            //    sb.Append($"{col.Name}");
            //}
            var cols = info.Columns.Select(col => $"{col.Name} {ConvertToDbType(col)}{(col.PrimaryKey ? " PRIMARY KEY" : "")}{(col.AutoIncrement ? " AUTOINCREMENT" : "")}{(col.NotNull || col.PrimaryKey ? " NOT NULL" : "")}{(col.Default != null ? $" DEFAULT '{col}'" : "")}");
            sb.Append(string.Join(",\n", cols)).Append(')');
            connection.Execute(sb.ToString());
        }

        internal override bool CheckTableExists(IDbConnection connection, DbTable dbTable)
        {
            var sql = $"SELECT count(*) FROM sqlite_master WHERE type='table' AND name = '{dbTable.Name}'";
            var count = connection.ExecuteScale(sql);
            return count != null && ((int)Convert.ChangeType(count, typeof(int))) > 0;
        }

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
    }
}

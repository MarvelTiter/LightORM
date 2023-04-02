using MDbContext.DbStruct;
using MDbEntity.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MDbContext.Extension
{
    internal static class TypeExtension
    {
        internal static DbTable CollectDbTableInfo(this Type tableType)
        {
            var tableName = tableType.GetAttribute<TableAttribute>()?.Name ?? tableType.Name;
            var props = tableType.GetProperties();
            var columns = new List<DbColumn>();
            foreach (var prop in props)
            {
                var ignore = prop.GetAttribute<IgnoreAttribute>();
                if (ignore != null) { continue; }
                var columnInfo = prop.GetAttribute<ColumnAttribute>();
                columns.Add(new DbColumn
                {
                    Name = columnInfo?.Name ?? prop.Name,
                    PrimaryKey = columnInfo?.PrimaryKey ?? false,
                    AutoIncrement = columnInfo?.AutoIncrement ?? false,
                    NotNull = columnInfo?.NotNull ?? false,
                    Length = columnInfo?.Length,
                    Default = columnInfo?.Default,
                    Comment = columnInfo?.Comment,
                    DataType = prop.PropertyType,
                });
            }
            return new DbTable { Name = tableName, Columns = columns };
        }
    }
}

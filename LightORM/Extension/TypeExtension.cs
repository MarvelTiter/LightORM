﻿using MDbContext.DbStruct;
using MDbEntity.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace MDbContext.Extension
{
    internal static class TypeExtension
    {
        internal static DbTable CollectDbTableInfo(this Type tableType)
        {
            var tableName = tableType.GetAttribute<TableAttribute>()?.Name ?? tableType.Name;
            var columns = CollectColumns(tableType);
            var indexs = CollectIndexs(tableType, columns);
            return new DbTable { Name = tableName, Columns = columns, Indexs = indexs };
        }

        private static List<DbIndex> CollectIndexs(Type tableType, List<DbColumn> columns)
        {
            IEnumerable<TableIndexAttribute> attrs = tableType.GetCustomAttributes(false).Where(a => a is TableIndexAttribute).Cast<TableIndexAttribute>();
            var indexs = new List<DbIndex>();
            foreach (TableIndexAttribute item in attrs)
            {
                indexs.Add(new()
                {
                    Columns = item.Indexs.Select(p => columns.FirstOrDefault(c => c.PropName == p).Name) ?? Enumerable.Empty<string>(),
                    DbIndexType = item.DbIndexType,
                    Name = item.Name
                });
            }
            return indexs;
        }

        private static List<DbColumn> CollectColumns(Type tableType)
        {
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
                    PropName = prop.Name,
                    PrimaryKey = columnInfo?.PrimaryKey ?? false,
                    AutoIncrement = columnInfo?.AutoIncrement ?? false,
                    NotNull = columnInfo?.NotNull ?? false,
                    Length = columnInfo?.Length,
                    Default = columnInfo?.Default,
                    Comment = columnInfo?.Comment,
                    DataType = prop.PropertyType,
                });
            }
            return columns;
        }
    }
}

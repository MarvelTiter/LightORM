using MDbContext.Extension;
using MDbEntity.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MDbContext.Utils
{
    internal class SimpleColumn
    {
        public string? DbColumn { get; set; }
        public string? PropName { get; set; }
    }
    internal static class EntityHelper
    {
        internal static IEnumerable<SimpleColumn> GetColumns(this Type entityType)
        {
            var props = entityType.GetProperties();
            foreach (var prop in props)
            {
                var ignore = prop.GetAttribute<IgnoreAttribute>();
                if (ignore != null) continue;
                var colAttr = prop.GetAttribute<ColumnAttribute>();
                yield return new SimpleColumn
                {
                    DbColumn = colAttr?.Name ?? prop.Name,
                    PropName = prop.Name,
                };
            }
        }
    }
}

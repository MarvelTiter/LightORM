using LightORM.DbEntity.Attributes;
using LightORM.Extension;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LightORM.Utils;

internal class SimpleColumn
{
    public string? DbColumn { get; set; }
    public string? PropName { get; set; }
    public bool Primary { get; set; }
    public bool AutoIncrement { get; set; }
    public bool NullValue { get; set; }
    public bool Insertable => !AutoIncrement && !NullValue;
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
                Primary = colAttr?.PrimaryKey ?? false,
                AutoIncrement = colAttr?.AutoIncrement ?? false,
            };
        }
    }
}

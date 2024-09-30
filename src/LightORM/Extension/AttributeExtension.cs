using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;
using System.Text;

namespace LightORM.Extension;

internal static class AttributeExtension
{
    public static T? GetAttribute<T>(this MemberInfo self, bool inherit = false) where T : Attribute
    {
        return Attribute.GetCustomAttribute(self, typeof(T), inherit) as T;
    }

    public static bool HasAttribute<T>(this MemberInfo self) where T : Attribute
    {
        return self.IsDefined(typeof(T), false);
    }

    public static IEnumerable<T> Select<T>(this DataTable table, Func<DataRow, T> selector)
    {
        foreach (DataRow row in table.Rows)
        {
            yield return selector(row);
        }
    }
}

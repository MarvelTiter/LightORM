using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace LightORM.Extension;

internal static class AttributeExtension
{
    public static T? GetAttribute<T>(this MemberInfo self) where T : Attribute
    {
        if (self.IsDefined(typeof(T), false))
        {
            var attrs = self.GetCustomAttributes(typeof(T), false);
            if (attrs.Length > 0 && attrs[0] is T)
            {
                return (T)attrs[0];
            }
        }
        return null;
    }

    public static bool HasAttribute<T>(this MemberInfo self) where T : Attribute
    {
        return self.IsDefined(typeof(T), false);
    }
}

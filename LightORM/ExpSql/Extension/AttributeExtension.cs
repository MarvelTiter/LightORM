using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Shared.ExpSql.Extension {
    public static class AttributeExtension {
        public static T GetAttribute<T>(this MemberInfo self) where T : Attribute {
            var attrs = self.GetCustomAttributes(typeof(T), false);
            if (attrs.Length > 0 && attrs[0] is T) {
                return (T)attrs[0];
            }
            return null;
        }
    }
}

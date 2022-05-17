using MDbEntity.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MDbContext.Utils
{
    public static class TypeExtensions
    {
        public static string GetColumnNameFromType(this Type self, string propertyName)
        {
            var prop = self.GetProperty(propertyName);
            var attr = Attribute.GetCustomAttribute(prop, typeof(ColumnNameAttribute));
            if (attr is ColumnNameAttribute col)
            {
                return col.Name;
            }
            return prop.Name;
        }
    }
}

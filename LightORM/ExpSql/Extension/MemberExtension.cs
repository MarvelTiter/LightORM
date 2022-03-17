using DExpSql;
using MDbEntity.Attributes;
using Shared.ExpSql.Extension;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace MDbContext.ExpSql.Extension
{
    internal static class MemberExtension
    {
        public static string GetColumnName(this MemberInfo self, SqlCaluse sqlCaluse, bool aliaRequest = true)
        {
            var table = self.DeclaringType;
            sqlCaluse.SetTableAlias(table);
            var alias = sqlCaluse.GetTableAlias(table);
            var attr = self.GetAttribute<ColumnNameAttribute>();
            if (attr != null)
            {
                if (aliaRequest)
                    return $"{alias}{attr.Name} {self.Name}";
                else
                    return $"{alias}{attr.Name}";
            }
            else
                return $"{alias}{self.Name}";
        }
    }
}

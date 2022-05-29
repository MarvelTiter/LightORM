using DExpSql;
using MDbContext.Extension;
using MDbEntity.Attributes;
using System.Reflection;

namespace MDbContext.ExpSql.Extension
{
    internal static class MemberExtension
    {

        public static string GetSelectColumnName(this MemberInfo self, SqlCaluse sqlCaluse, bool aliaRequest = true)
        {
            var table = self.DeclaringType;
            sqlCaluse.SetTableAlias(table);
            var alias = sqlCaluse.GetTableAlias(table);
            var attr = self.GetAttribute<ColumnNameAttribute>();
            var colAlias = self.Name;
            if (sqlCaluse.GetMemberName != null && !string.IsNullOrEmpty(sqlCaluse.GetMemberName()))
            {
                colAlias = sqlCaluse.GetMemberName();
            }
            if (attr != null)
            {
                if (aliaRequest)
                    return $"{alias}{attr.Name} {colAlias}";
                else
                    return $"{alias}{attr.Name}";
            }
            else
                return $"{alias}{colAlias}";
        }
        public static string GetColumnName(this MemberInfo self, SqlCaluse sqlCaluse, bool aliaRequest = true)
        {
            var table = self.DeclaringType;
            sqlCaluse.SetTableAlias(table);
            var alias = sqlCaluse.GetTableAlias(table);
            var attr = self.GetAttribute<ColumnNameAttribute>();
            var colAlias = self.Name;            
            if (attr != null)
            {
                if (aliaRequest)
                    return $"{alias}{attr.Name} {colAlias}";
                else
                    return $"{alias}{attr.Name}";
            }
            else
                return $"{alias}{colAlias}";
        }
    }
}

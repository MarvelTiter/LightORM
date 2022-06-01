using DExpSql;
using MDbContext.Extension;
using MDbContext.NewExpSql;
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
        public static string GetColumnName(this MemberInfo self, SqlContext
            context, SqlConfig config)
        {
            var table = self.DeclaringType;
            var alias = context.GetTableAlias(table.Name);
            var attr = self.GetAttribute<ColumnNameAttribute>();
            var colAlias = self.Name;
            var comma = config.RequiredComma ? "," : "";
            if (config.RequiredColumnAlias && attr != null)
            {
                if (config.RequiredTableAlias)
                    return $"{alias}.{attr.Name} {colAlias}{comma}";
                else
                    return $"{attr.Name} {colAlias}{comma}";
            }
            else
            {
                if (config.RequiredTableAlias)
                    return $"{alias}.{colAlias}{comma}";
                else
                    return $"{colAlias}{comma}";
            }
        }
    }
}

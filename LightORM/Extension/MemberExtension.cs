using DExpSql;
using MDbContext.ExpressionSql;
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
        public static string GetColumnName(this SqlFieldInfo column, SqlContext context, SqlConfig config)
        {
            var tableAlias = config.RequiredTableAlias ? $"{column.TableAlias}." : "";
            var dbColumn = config.RequiredEmphasis ? context.DbHandler.DbEmphasis(column.FieldName ?? "") : column.FieldName;
            var columnAlias = config.RequiredColumnAlias ? $" {column.FieldAlias}" : "";
            var comma = config.RequiredComma ? ", " : "";
            return $"{tableAlias}{dbColumn}{columnAlias}{comma}";            
        }
        public static string GetColumnName(this MemberInfo self, SqlContext context, SqlConfig config)
        {
            var column = context.GetColumn(self.DeclaringType, self.Name);
            var tableAlias = config.RequiredTableAlias ? $"{column.TableAlias}." : "";
            var dbColumn = config.RequiredEmphasis ? context.DbHandler.DbEmphasis(column.FieldName ?? "") : column.FieldName;
            var columnAlias = config.RequiredColumnAlias ? $" {column.FieldAlias}" : "";
            var comma = config.RequiredComma ? ", " : "";
            return $"{tableAlias}{dbColumn}{columnAlias}{comma}";
            //if (config.RequiredEmphasis)
            //{
            //    if (config.RequiredTableAlias)
            //        return $"{column?.TableAlias}.{context.DbHandler.ColumnEmphasis(column?.FieldName ?? "")}";
            //    else
            //        return context.DbHandler.ColumnEmphasis(column?.FieldName ?? "") + "";
            //}
            //else
            //{
            //    if (config.RequiredTableAlias)
            //        return $"{column?.TableAlias}.{column?.FieldName}";
            //    else
            //        return $"{column?.FieldName}";
            //}
        }
    }
}

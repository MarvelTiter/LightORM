using LightORM.ExpressionSql;
using System.Reflection;

namespace LightORM.Extension;

internal static class MemberExtension
{
    public static string GetColumnName(this SqlFieldInfo column, SqlContext context, SqlConfig config)
    {
        var tableAlias = config.RequiredTableAlias ? $"{column.TableAlias}." : "";
        var dbColumn = config.RequiredEmphasis ? context.DbHandler.DbEmphasis(column.FieldName ?? "") : column.FieldName;
        var columnAlias = config.RequiredColumnAlias ? $" {context.DbHandler.GetColumnEmphasis(true)}{column.FieldAlias}{context.DbHandler.GetColumnEmphasis(false)}" : "";
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

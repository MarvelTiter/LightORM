using LightORM.DbEntity.Attributes;
using LightORM.ExpressionSql;
using LightORM.Extension;
using System.Linq.Expressions;
using System.Reflection;

namespace LightORM.ExpressionSql.ExpressionVisitor;

internal class ParameterExpVisitor : BaseVisitor<ParameterExpression>
{
    public override void DoVisit(ParameterExpression exp, SqlConfig config, SqlContext context)
    {
        var t = exp.Type;
        var props = t.GetProperties();
        foreach (PropertyInfo item in props)
        {
            //var field = context.GetColumn(t, item.Name);
            //if (field == null) continue;
            //if (config.RequiredColumnAlias)
            //    context += $"{field?.TableAlias}.{context.DbHandler.ColumnEmphasis(field?.FieldName ?? "")} {field?.FieldAlias},";
            //else
            //    context += ($"{field?.TableAlias}.{context.DbHandler.ColumnEmphasis(field?.FieldName ?? "")},");
            if (item.HasAttribute<IgnoreAttribute>()) continue;
            var col = context.GetColumn(item.DeclaringType, item.Name);
            var field = col.GetColumnName(context, config);
            context += field;
            context.AddFieldName(field);
            context.AddCell(new UnitCell
            {
                TableAlias = col.TableAlias,
                ColumnName = col.FieldName,
                ColumnAlias = col.FieldAlias,
                IsPrimaryKey = col.IsPrimaryKey,
            });
        }
    }
}

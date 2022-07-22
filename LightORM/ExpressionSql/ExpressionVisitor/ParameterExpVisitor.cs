using MDbContext.ExpressionSql;
using MDbContext.ExpressionSql.ExpressionVisitor;
using MDbContext.Extension;
using MDbEntity.Attributes;
using System.Linq.Expressions;
using System.Reflection;

namespace MDbContext.NewExpSql.ExpressionVisitor
{
    internal class ParameterExpVisitor : BaseVisitor<ParameterExpression>
    {
        public override void DoVisit(ParameterExpression exp, SqlConfig config, SqlContext context)
        {
            var t = exp.Type;
            var props = t.GetProperties();
            foreach (PropertyInfo item in props)
            {
                var field = context.GetColumn(t, item.Name);
                if (field == null) continue;
                if (config.RequiredColumnAlias)
                    context += ($"{field.TableAlias}.`{field.FieldName}` {field.FieldAlias},");
                else
                    context += ($"{field.TableAlias}.`{field.FieldName}`,");
            }
        }
    }
}

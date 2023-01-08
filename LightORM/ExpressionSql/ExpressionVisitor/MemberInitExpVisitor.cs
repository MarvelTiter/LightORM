using MDbContext.ExpSql.Extension;
using System.Linq.Expressions;
using System.Reflection;

namespace MDbContext.ExpressionSql.ExpressionVisitor;

internal class MemberInitExpVisitor : BaseVisitor<MemberInitExpression>
{
    public override void DoVisit(MemberInitExpression exp, SqlConfig config, SqlContext context)
    {
        var bindings = exp.Bindings;
        if ((bindings?.Count) > 0)
        {
            foreach (MemberBinding binding in bindings)
            {
                if (binding is MemberAssignment memberExp)
                {
                    var name = binding.Member.Name;
                    var value = Expression.Lambda(memberExp.Expression).Compile().DynamicInvoke();
                    context += $" {name ?? binding.Member.Name} = ";
                    context.AppendDbParameter(value);
                    context += ",\n";
                }
            }
        }
        else
        {
            var t = exp.Type;
            var props = t.GetProperties();
            foreach (PropertyInfo item in props)
            {
                var field = item.GetColumnName(context, config);
                context += field;
                //var field = context.GetColumn(t, item.Name);
                //if (field == null) continue;
                //if (config.RequiredColumnAlias)
                //    context += $"{field.TableAlias}.{field.FieldName} {field.FieldAlias},";
                //else
                //    context += $"{field.TableAlias}.{field.FieldName},";
            }
        }
        if (context.EndWith(",\n"))
        {
            context -= ",\n";
        }
    }
}

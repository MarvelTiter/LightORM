using MDbContext.Utils;
using System;
using System.Linq.Expressions;

namespace MDbContext.ExpressionSql.ExpressionVisitor;

internal class NewExpVisitor : BaseVisitor<NewExpression>
{
    public override void DoVisit(NewExpression exp, SqlConfig config, SqlContext context)
    {
        if (config.RequiredValue)
        {
            // update =>  () => new { xxxx }
            // insert 
            for (int i = 0; i < exp.Members!.Count; i++)
            {
                var member = exp.Members[i];
                var colName = context.GetColumn(context.MainTable!.Type!, member.Name);
                var arg = exp.Arguments[i];
                var func = Expression.Lambda(arg).Compile();
                var value = func.DynamicInvoke();
                if (value == null || value == DBNull.Value)
                    continue;
                context.AddEntityField(colName?.FieldName ?? member.Name, value);
            }
        }
        else
        {
            // select
            var len = exp.Arguments.Count;
            for (int i = 0; i < len; i++)
            {
                var argExp = exp.Arguments[i];
                var member = exp.Members?[i];
                ExpressionVisit.Visit(argExp, config, context);
                if (argExp.Type.IsClass && argExp.Type != typeof(string))
                    continue;
                if (config.RequiredColumnAlias)
                {
                    context.Append(" ");
                    context.Append(member?.Name ?? "");
                }
                if (config.RequiredComma)
                {
                    context.Append(",");
                }
            }
        }
    }
}

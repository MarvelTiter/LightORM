﻿using System.Linq.Expressions;

namespace LightORM.ExpressionSql.ExpressionVisitor;

internal class NewArrayExpVisitor : BaseVisitor<NewArrayExpression>
{
    public override void DoVisit(NewArrayExpression exp, SqlResolveOptions config, SqlContext context)
    {
        var count = exp.Expressions.Count;
        for (int i = 0; i < count; i++)
        {
            var item = exp.Expressions[i];
            ExpressionVisit.Visit(item, config, context);
            if (i < count - 1)
                context.Append(", ");
        }
        //foreach (var item in exp.Expressions)
        //{
        //    ExpressionVisit.Visit(item, config, context);
        //    context.Append(", ");
        //}
    }
}

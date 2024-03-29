﻿using System.Linq.Expressions;

namespace LightORM.ExpressionSql.ExpressionVisitor;

internal class LambdaExpVisitor : BaseVisitor<LambdaExpression>
{
    public override void DoVisit(LambdaExpression exp, SqlResolveOptions config, SqlContext context)
    {
        ExpressionVisit.Visit(exp.Body, config, context);
    }
}

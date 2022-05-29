﻿using System.Linq.Expressions;

namespace MDbContext.NewExpSql.ExpressionVisitor
{
    internal class ConstantExpVisitor : BaseVisitor<ConstantExpression>
    {
        public override void DoVisit(ConstantExpression exp, SqlConfig config, ISqlContext context)
        {
            var value = exp.Value;
            context.AppendDbParameter(value);
        }
    }
}

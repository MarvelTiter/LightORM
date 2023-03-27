using MDbContext.NewExpSql.ExpressionVisitor;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace MDbContext.ExpressionSql.ExpressionVisitor;

internal class ExpressionVisit
{
    public static void Visit(Expression exp, SqlConfig config, SqlContext context)
        => GetVisitor(exp).Visit(exp, config, context);

    private static IExpressionVisitor GetVisitor(Expression exp)
    {
        IExpressionVisitor _i;
        if (exp == null)
            throw new ArgumentNullException("Expression", "不能为null");
        //_i = exp switch
        //{
        //    BinaryExpression => new BinaryExpVisitor(),
        //    ConstantExpression => new ConstantExpVisitor(),
        //    MemberExpression => new MemberExpVisitor(),
        //    MethodCallExpression => new MethodCallExpVisitor(),
        //    NewArrayExpression => new NewArrayExpVisitor(),
        //    NewExpression => new NewExpVisitor(),
        //    UnaryExpression => new UnaryExpVisitor(),
        //    ParameterExpression => new ParameterExpVisitor(),
        //    LambdaExpression => new LambdaExpVisitor(),
        //    MemberInitExpression => new MemberInitExpVisitor(),
        //    _ => throw new ArgumentException("不支持的Expression")
        //};

        if (exp is BinaryExpression)
            _i = new BinaryExpVisitor();
        else if (exp is ConstantExpression)
            _i = new ConstantExpVisitor();
        else if (exp is MemberExpression)
            _i = new MemberExpVisitor();
        else if (exp is MethodCallExpression)
            _i = new MethodCallExpVisitor();
        else if (exp is NewArrayExpression)
            _i = new NewArrayExpVisitor();
        else if (exp is NewExpression)
            _i = new NewExpVisitor();
        else if (exp is UnaryExpression)
            _i = new UnaryExpVisitor();
        else if (exp is ParameterExpression)
            _i = new ParameterExpVisitor();
        else if (exp is MemberInitExpression)
            _i = new MemberInitExpVisitor();
        else if (exp is LambdaExpression)
            _i = new LambdaExpVisitor();
        else
            throw new ArgumentException($"不支持的Expression => {exp}");

        return _i;
    }
}

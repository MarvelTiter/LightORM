using MDbContext.NewExpSql.ExpressionParamParser;
using MDbContext.NewExpSql.SqlFragment;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace MDbContext.NewExpSql
{
    internal class ExpressionParamVisit
    {
        public static void Where(Expression exp, BaseFragment fragment) => GetParser(exp).Where(exp, fragment);

        private static IExpressionParser GetParser(Expression exp)
        {
            IExpressionParser _i;
            var expType = exp.GetType();
            if (exp == null)
                throw new ArgumentNullException("Expression", "不能为null");
            else if (exp is BinaryExpression)
                _i = new BinaryParamParser();
            else if (exp is ConstantExpression)
                _i = new ConstantParamParser();
            else if (exp is MemberExpression)
                _i = new MemberParamParser();
            else if (exp is MethodCallExpression)
                _i = new MethodCallParamParser();
            else if (exp is NewArrayExpression)
                _i = new NewArrayParamParser();
            else if (exp is NewExpression)
                _i = new NewParamParser();
            else if (exp is UnaryExpression)
                _i = new UnaryParamParser();
            else if (exp is ParameterExpression)
                _i = new ParameterParamParser();
            else if (exp is LambdaExpression)
                _i = new LambdaParamParser();
            else if (exp is MemberInitExpression)
                _i = new MemberInitParamParser();
            else
                throw new ArgumentException("不支持的Expression");
            return _i;
        }
    }
}

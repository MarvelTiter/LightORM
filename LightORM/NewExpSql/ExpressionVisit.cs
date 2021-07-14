using MDbContext.NewExpSql.ExpressionParser;
using MDbContext.NewExpSql.SqlFragment;
using System;
using System.Linq;
using System.Linq.Expressions;

namespace MDbContext.NewExpSql {
    internal class ExpressionVisit {

        public static void Select(Expression exp, ISqlContext context, BaseFragment fragment) => GetParser(exp).Select(exp,context, fragment);
        public static void Join(Expression exp, ISqlContext context, BaseFragment fragment) => GetParser(exp).Join(exp, context, fragment);
        public static void Where(Expression exp, ISqlContext context, BaseFragment fragment) => GetParser(exp).Where(exp, context, fragment);




        private static IExpressionParser GetParser(Expression exp) {
            IExpressionParser _i;
            var expType = exp.GetType();
            if (exp == null)
                throw new ArgumentNullException("Expression", "不能为null");
            else if (exp is BinaryExpression)
                _i = new BinaryExpressionParser();
            else if (exp is ConstantExpression)
                _i = new ConstantExpressionParser();
            else if (exp is MemberExpression)
                _i = new MemberExpressionParser();
            else if (exp is MethodCallExpression)
                _i = new MethodCallExpressionParser();
            else if (exp is NewArrayExpression)
                _i = new NewArrayExpressionParser();
            else if (exp is NewExpression)
                _i = new NewExpressionParser();
            else if (exp is UnaryExpression)
                _i = new UnaryExpressionParser();
            else if (exp is ParameterExpression)
                _i = new ParameterExpressionParser();
            else if (exp is LambdaExpression)
                _i = new LambdaExpressionParser();
            else if (exp is MemberInitExpression)
                _i = new MemberInitExpressionParser();
            else
                throw new ArgumentException("不支持的Expression");

            return _i;
        }
    }
}

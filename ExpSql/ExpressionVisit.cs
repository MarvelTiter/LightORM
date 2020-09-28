using DExpSql.ExpressionHandle;
using ExpSql.ExpressionHandle;
using System;
using System.Linq.Expressions;

namespace DExpSql
{
    internal class ExpressionVisit
    {
        public static void Update(Expression exp, SqlCaluse SqlCaluse) => GetExpressionHandler(exp).Update(exp, SqlCaluse);
        public static void PrimaryKey(Expression exp, SqlCaluse sqlCaluse) => GetExpressionHandler(exp).PrimaryKey(exp, sqlCaluse);

        public static void Select(Expression exp, SqlCaluse SqlCaluse) => GetExpressionHandler(exp).Select(exp, SqlCaluse);

        public static void Join(Expression exp, SqlCaluse SqlCaluse) => GetExpressionHandler(exp).Join(exp, SqlCaluse);

        public static void Where(Expression exp, SqlCaluse SqlCaluse) => GetExpressionHandler(exp).Where(exp, SqlCaluse);

        public static void Insert(Expression exp, SqlCaluse sqlCaluse) => GetExpressionHandler(exp).Insert(exp, sqlCaluse);

        public static void In(Expression exp, SqlCaluse SqlCaluse) => GetExpressionHandler(exp).In(exp, SqlCaluse);

        public static void GroupBy(Expression exp, SqlCaluse SqlCaluse) => GetExpressionHandler(exp).GroupBy(exp, SqlCaluse);

        public static void OrderBy(Expression exp, SqlCaluse SqlCaluse) => GetExpressionHandler(exp).OrderBy(exp, SqlCaluse);


        private static IExpressionSql GetExpressionHandler(Expression exp)
        {
            IExpressionSql _i;
            var expType = exp.GetType();
            if (exp == null)
                throw new ArgumentNullException("Expression", "不能为null");
            else if (exp is BinaryExpression)
                _i = new BinaryExpressionCaluse();
            else if (exp is ConstantExpression)
                _i = new ConstantExpressionCaluse();
            else if (exp is MemberExpression)
                _i = new MemberExpressionCaluse();
            else if (exp is MethodCallExpression)
                _i = new MethodCallExpressionCaluse();
            else if (exp is NewArrayExpression)
                _i = null;
            else if (exp is NewExpression)
                _i = new NewExpressionCaluse();
            else if (exp is UnaryExpression)
                _i = new UnaryExpressionCaluse();
            else if (exp is ParameterExpression)
                _i = new ParameterExpressionCaluse() ;
            else
                throw new ArgumentException("不支持的Expression");

            return _i;
        }
    }
}

using MDbContext.NewExpSql.SqlFragment;
using System;
using System.Linq.Expressions;

namespace MDbContext.NewExpSql.ExpressionParamParser
{
    internal class BaseParser<T> : IExpressionParser where T : Expression
    {
        private string _expressionType => typeof(T).Name;

        #region Insert
        public BaseFragment Insert(Expression expression, BaseFragment BaseFragment)
            => Insert((T)expression, (InsertFragment)BaseFragment);
        protected virtual BaseFragment Insert(T exp, InsertFragment fragment) => throw new NotImplementedException($"[{_expressionType}] 未实现 Insert");
        #endregion

        #region Update
        public BaseFragment Update(Expression expression, BaseFragment BaseFragment)
            => Update((T)expression, (UpdateFragment)BaseFragment);
        protected virtual BaseFragment Update(T exp, UpdateFragment fragment) => throw new NotImplementedException($"[{_expressionType}] 未实现 Update");
        #endregion

        #region Where
        public BaseFragment Where(Expression expression, BaseFragment BaseFragment)
            => Where((T)expression, (WhereFragment)BaseFragment);
        public virtual BaseFragment Where(T exp, WhereFragment fragment) => throw new NotImplementedException($"[{_expressionType}] 未实现 Where");
        #endregion

        #region 无需解析参数的子句
        public BaseFragment Select(Expression expression, BaseFragment SqlFragment)
        {
            throw new NotImplementedException();
        }

        public BaseFragment Join(Expression expression, BaseFragment SqlFragment)
        {
            throw new NotImplementedException();
        }

        public BaseFragment GroupBy(Expression expression, BaseFragment SqlFragment)
        {
            throw new NotImplementedException();
        }

        public BaseFragment OrderBy(Expression expression, BaseFragment SqlFragment)
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}

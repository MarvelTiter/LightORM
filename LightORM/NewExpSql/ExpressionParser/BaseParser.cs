using MDbContext.NewExpSql.SqlFragment;
using System;
using System.Linq.Expressions;

namespace MDbContext.NewExpSql.ExpressionParser
{
    internal class BaseParser<T> : IExpressionParser where T : Expression
    {
        private string _expressionType => typeof(T).Name;

        #region Select
        public BaseFragment Select(Expression expression, BaseFragment BaseFragment)
            => Select((T)expression, (SelectFragment)BaseFragment);
        protected virtual BaseFragment Select(T exp, SelectFragment fragment) => throw new NotImplementedException($"[{_expressionType}] 未实现 Select");
        #endregion

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

        #region Join
        public BaseFragment Join(Expression expression, BaseFragment BaseFragment)
            => Join((T)expression, (JoinFragment)BaseFragment);
        public virtual BaseFragment Join(T exp, JoinFragment fragment) => throw new NotImplementedException($"[{_expressionType}] 未实现 Join");
        #endregion

        #region Where
        public BaseFragment Where(Expression expression, BaseFragment BaseFragment)
            => Where((T)expression, (WhereFragment)BaseFragment);
        public virtual BaseFragment Where(T exp, WhereFragment fragment) => throw new NotImplementedException($"[{_expressionType}] 未实现 Where");
        #endregion

        #region GroupBy
        public BaseFragment GroupBy(Expression expression, BaseFragment BaseFragment)
            => GroupBy((T)expression, (GroupByFragment)BaseFragment);
        protected virtual BaseFragment GroupBy(T exp, GroupByFragment fragment) => throw new NotImplementedException($"[{_expressionType}] 未实现 GroupBy");
        #endregion

        #region OrderBy
        public BaseFragment OrderBy(Expression expression, BaseFragment BaseFragment)
            => Where((T)expression, (OrderByFragment)BaseFragment);
        public virtual BaseFragment OrderBy(T exp, OrderByFragment fragment) => throw new NotImplementedException($"[{_expressionType}] 未实现 OrderBy");
        #endregion

    }
}

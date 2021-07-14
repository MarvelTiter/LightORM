using MDbContext.NewExpSql.SqlFragment;
using System;
using System.Linq.Expressions;

namespace MDbContext.NewExpSql.ExpressionParser {
    internal class BaseParser<T> : IExpressionParser where T : Expression {
        private string _expressionType => typeof(T).Name;

        #region Select
        public BaseFragment Select(Expression expression, ISqlContext context, BaseFragment BaseFragment)
            => Select((T)expression, context, (SelectFragment)BaseFragment);
        protected virtual BaseFragment Select(T exp, ISqlContext context, SelectFragment fragment) => throw new NotImplementedException($"[{_expressionType}] 未实现 Select");
        #endregion

        #region Insert
        public BaseFragment Insert(Expression expression, ISqlContext context, BaseFragment BaseFragment)
            => Insert((T)expression, context, (InsertFragment)BaseFragment);
        protected virtual BaseFragment Insert(T exp, ISqlContext context, InsertFragment fragment) => throw new NotImplementedException($"[{_expressionType}] 未实现 Insert");
        #endregion

        #region Update
        public BaseFragment Update(Expression expression, ISqlContext context, BaseFragment BaseFragment)
            => Update((T)expression, context, (UpdateFragment)BaseFragment);
        protected virtual BaseFragment Update(T exp, ISqlContext context, UpdateFragment fragment) => throw new NotImplementedException($"[{_expressionType}] 未实现 Update");
        #endregion

        #region Join
        public BaseFragment Join(Expression expression, ISqlContext context, BaseFragment BaseFragment)
            => Join((T)expression, context, (JoinFragment)BaseFragment);
        public virtual BaseFragment Join(T exp, ISqlContext context, JoinFragment fragment) => throw new NotImplementedException($"[{_expressionType}] 未实现 Join");
        #endregion

        #region Where
        public BaseFragment Where(Expression expression, ISqlContext context, BaseFragment BaseFragment)
            => Where((T)expression, context, (WhereFragment)BaseFragment);
        public virtual BaseFragment Where(T exp, ISqlContext context, WhereFragment fragment) => throw new NotImplementedException($"[{_expressionType}] 未实现 Where");
        #endregion

        #region GroupBy
        public BaseFragment GroupBy(Expression expression, ISqlContext context, BaseFragment BaseFragment)
            => GroupBy((T)expression, context, (GroupByFragment)BaseFragment);
        protected virtual BaseFragment GroupBy(T exp, ISqlContext context, GroupByFragment fragment) => throw new NotImplementedException($"[{_expressionType}] 未实现 GroupBy");
        #endregion

        #region OrderBy
        public BaseFragment OrderBy(Expression expression, ISqlContext context, BaseFragment BaseFragment)
            => Where((T)expression, context, (OrderByFragment)BaseFragment);
        public virtual BaseFragment OrderBy(T exp, ISqlContext context, OrderByFragment fragment) => throw new NotImplementedException($"[{_expressionType}] 未实现 OrderBy");
        #endregion

    }
}

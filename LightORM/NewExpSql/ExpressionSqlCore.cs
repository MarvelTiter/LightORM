using MDbContext.NewExpSql.SqlFragment;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace MDbContext.NewExpSql
{
    public class ExpressionSqlCore<T>
    {
        private readonly DbBaseType type;
        internal ISqlContext Context { get; }
        private ITableContext tables;
        public ExpressionSqlCore(DbBaseType type)
        {
            Context = new SqlContext();
            this.type = type;
        }

        #region Select
        public ExpressionSqlCore<T> Select(Expression<Func<T, object>> exp, bool distanct)
        {
            if (exp == null)
            {
                exp = t => new { t };
            }
            var expKey = $"{typeof(T).Name}{exp}";
            var part = BaseFragment.GetPart<SelectFragment>(expKey, distanct);//new SelectFragment(Context, distanct);//
            part.SetTables<T>(ref tables, type);
            part.ResolveSql(exp.Body, typeof(T));
            Context.AddFragment(part);
            return this;
        }

        public ExpressionSqlCore<T> Select<T1>(Expression<Func<T, T1, object>> exp, bool distanct)
        {
            if (exp == null)
            {
                exp = (t, t1) => new { t, t1 };
            }
            var expKey = $"{typeof(T).Name}{typeof(T1).Name}{exp}";
            var part = BaseFragment.GetPart<SelectFragment>(expKey, distanct);//new SelectFragment(distanct);//
            part.SetTables<T>(ref tables, type);
            part.ResolveSql(exp.Body, typeof(T), typeof(T1));
            Context.AddFragment(part);
            return this;
        }
        #endregion

        #region Join
        // inner join
        public ExpressionSqlCore<T> InnerJoin<T1>(Expression<Func<T, T1, bool>> exp)
        {
            HandleJoinPart<T1>(exp, JoinType.Inner);
            return this;
        }
        public ExpressionSqlCore<T> InnerJoin<T1, T2>(Expression<Func<T, T1, T2, bool>> exp)
        {
            HandleJoinPart<T1>(exp, JoinType.Inner);
            return this;
        }
        public ExpressionSqlCore<T> InnerJoin<T1, T2>(Expression<Func<T1, T2, bool>> exp)
        {
            HandleJoinPart<T1>(exp, JoinType.Inner);
            return this;
        }
        // left join
        public ExpressionSqlCore<T> LeftJoin<T1>(Expression<Func<T, T1, bool>> exp)
        {
            HandleJoinPart<T1>(exp, JoinType.Left);
            return this;
        }
        public ExpressionSqlCore<T> LeftJoin<T1, T2>(Expression<Func<T, T1, T2, bool>> exp)
        {
            HandleJoinPart<T1>(exp, JoinType.Left);
            return this;
        }
        public ExpressionSqlCore<T> LeftJoin<T1, T2>(Expression<Func<T1, T2, bool>> exp)
        {
            HandleJoinPart<T1>(exp, JoinType.Left);
            return this;
        }
        // right join
        public ExpressionSqlCore<T> RightJoin<T1>(Expression<Func<T, T1, bool>> exp)
        {
            HandleJoinPart<T1>(exp, JoinType.Right);
            return this;
        }
        public ExpressionSqlCore<T> RightJoin<T1, T2>(Expression<Func<T, T1, T2, bool>> exp)
        {
            HandleJoinPart<T1>(exp, JoinType.Right);
            return this;
        }
        public ExpressionSqlCore<T> RightJoin<T1, T2>(Expression<Func<T1, T2, bool>> exp)
        {
            HandleJoinPart<T1>(exp, JoinType.Right);
            return this;
        }

        private void HandleJoinPart<JoinTable>(LambdaExpression exp, JoinType joinType)
        {
            JoinFragment part = BaseFragment.GetPart<JoinFragment>(exp.ToString(), joinType, typeof(JoinTable));//new JoinFragment(JoinType.Inner, typeof(JoinTable));//
            part.SetTables<T>(ref tables, type);
            part.ResolveSql(exp.Body);
            Context.AddFragment(part);
        }

        #endregion

        #region Where

        public ExpressionSqlCore<T> Where(Expression<Func<T, object>> exp)
        {
            HandleWherePart(exp.Body);
            return this;
        }

        public ExpressionSqlCore<T> Where<T1>(Expression<Func<T, T1, object>> exp)
        {
            HandleWherePart(exp.Body);
            return this;
        }
        public ExpressionSqlCore<T> Where<T1>(Expression<Func<T1, object>> exp)
        {
            HandleWherePart(exp.Body);
            return this;
        }

        public ExpressionSqlCore<T> Where<T1, T2>(Expression<Func<T, T1, T2, object>> exp)
        {
            HandleWherePart(exp.Body);
            return this;
        }
        public ExpressionSqlCore<T> Where<T1, T2>(Expression<Func<T1, T2, object>> exp)
        {
            HandleWherePart(exp.Body);
            return this;
        }

        public ExpressionSqlCore<T> Where<T1, T2, T3>(Expression<Func<T, T1, T2, T3, object>> exp)
        {
            HandleWherePart(exp.Body);
            return this;
        }
        public ExpressionSqlCore<T> Where<T1, T2, T3>(Expression<Func<T1, T2, T3, object>> exp)
        {
            HandleWherePart(exp.Body);
            return this;
        }
        private void HandleWherePart(Expression exp)
        {
            WhereFragment part = new WhereFragment();// BaseFragment.GetPart<WhereFragment>(exp.ToString(), Context);
            part.ResolveSql(exp);
            Context.AddFragment(part);
        }
        #endregion
    }
}

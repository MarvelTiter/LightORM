using MDbContext.NewExpSql.SqlFragment;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace MDbContext.NewExpSql {
    public class ExpressionSqlCore<T> {
        //private List<SqlFragment> fragments;

        internal ISqlContext Context { get; }
        public ExpressionSqlCore(DbBaseType type) {
            Context = new ExpressionSqlCoreContext<T>(type);
        }

        #region Select
        public ExpressionSqlCore<T> Select(Expression<Func<T, object>> exp, bool distanct) {
            if (exp == null) {
                exp = t => new { t };
            }
            var part = new SelectFragment(Context, distanct);//BaseFragment.GetPart<SelectFragment>(exp.ToString(), Context, distanct);
            part.ResolveSql(exp.Body, typeof(T));
            Context.AddFragment(part);
            return this;
        }

        public ExpressionSqlCore<T> Select<T1>(Expression<Func<T, T1, object>> exp, bool distanct) {
            if (exp == null) {
                exp = (t, t1) => new { t, t1 };
            }
            var part = new SelectFragment(Context, distanct);//BaseFragment.GetPart<SelectFragment>(exp.ToString(), Context, distanct);
            part.ResolveSql(exp.Body, typeof(T), typeof(T1));
            Context.AddFragment(part);
            return this;
        }
        #endregion

        #region Join
        // inner join
        public ExpressionSqlCore<T> InnerJoin<T1>(Expression<Func<T, T1, bool>> exp) {           
            HandleJoinPart<T1>(exp.Body, JoinType.Inner);
            return this;
        }
        public ExpressionSqlCore<T> InnerJoin<T1, T2>(Expression<Func<T, T1, T2, bool>> exp) {
            HandleJoinPart<T1>(exp.Body, JoinType.Inner);
            return this;
        }
        public ExpressionSqlCore<T> InnerJoin<T1, T2>(Expression<Func<T1, T2, bool>> exp) {
            HandleJoinPart<T1>(exp.Body, JoinType.Inner);            
            return this;
        }
        // left join
        public ExpressionSqlCore<T> LeftJoin<T1>(Expression<Func<T, T1, bool>> exp) {
            HandleJoinPart<T1>(exp.Body, JoinType.Left);
            return this;
        }
        public ExpressionSqlCore<T> LeftJoin<T1, T2>(Expression<Func<T, T1, T2, bool>> exp) {
            HandleJoinPart<T1>(exp.Body, JoinType.Left);
            return this;
        }
        public ExpressionSqlCore<T> LeftJoin<T1, T2>(Expression<Func<T1, T2, bool>> exp) {
            HandleJoinPart<T1>(exp.Body, JoinType.Left);
            return this;
        }
        // right join
        public ExpressionSqlCore<T> RightJoin<T1>(Expression<Func<T, T1, bool>> exp) {
            HandleJoinPart<T1>(exp.Body, JoinType.Right);
            return this;
        }
        public ExpressionSqlCore<T> RightJoin<T1, T2>(Expression<Func<T, T1, T2, bool>> exp) {
            HandleJoinPart<T1>(exp.Body, JoinType.Right);
            return this;
        }
        public ExpressionSqlCore<T> RightJoin<T1, T2>(Expression<Func<T1, T2, bool>> exp) {
            HandleJoinPart<T1>(exp.Body, JoinType.Right);
            return this;
        }

        private void HandleJoinPart<JoinTable>(Expression body, JoinType joinType) {
            JoinFragment part = new JoinFragment(Context, JoinType.Inner, typeof(JoinTable));//BaseFragment.GetPart<JoinFragment>(exp.ToString(), Context, JoinType.Inner, typeof(T1));
            part.ResolveSql(body);
            Context.AddFragment(part);
        }

        #endregion

        #region Where

        public ExpressionSqlCore<T> Where(Expression<Func<T, object>> exp) {
            HandleWherePart(exp.Body);
            return this;
        }

        public ExpressionSqlCore<T> Where<T1>(Expression<Func<T, T1, object>> exp) {
            HandleWherePart(exp.Body);
            return this;
        }
        public ExpressionSqlCore<T> Where<T1>(Expression<Func<T1, object>> exp) {
            HandleWherePart(exp.Body);
            return this;
        }

        public ExpressionSqlCore<T> Where<T1, T2>(Expression<Func<T, T1, T2, object>> exp) {
            HandleWherePart(exp.Body);
            return this;
        }
        public ExpressionSqlCore<T> Where<T1, T2>(Expression<Func<T1, T2, object>> exp) {
            HandleWherePart(exp.Body);
            return this;
        }

        public ExpressionSqlCore<T> Where<T1, T2, T3>(Expression<Func<T, T1, T2, T3, object>> exp) {
            HandleWherePart(exp.Body);
            return this;
        }
        public ExpressionSqlCore<T> Where<T1, T2, T3>(Expression<Func<T1, T2, T3, object>> exp) {
            HandleWherePart(exp.Body);
            return this;
        }
        private void HandleWherePart(Expression exp) {
            Context.WhereIndex++;
            WhereFragment part = new WhereFragment(Context);// BaseFragment.GetPart<WhereFragment>(exp.ToString(), Context);
            part.ResolveSql(exp);
            Context.AddFragment(part);
        }
        #endregion
    }
}

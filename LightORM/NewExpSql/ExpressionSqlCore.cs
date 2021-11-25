using MDbContext.NewExpSql.SqlFragment;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using System.Text.RegularExpressions;

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
            HandleSelect(exp, distanct, typeof(T));
            return this;
        }

        public ExpressionSqlCore<T> Select<T1>(Expression<Func<T, T1, object>> exp, bool distanct)
        {
            if (exp == null)
            {
                exp = (t, t1) => new { t, t1 };
            }
            HandleSelect(exp, distanct, typeof(T), typeof(T1));
            return this;
        }

        public ExpressionSqlCore<T> Select<T1, T2>(Expression<Func<T, T1, T2, object>> exp, bool distanct)
        {
            if (exp == null)
            {
                exp = (t, t1, t2) => new { t, t1, t2 };
            }
            HandleSelect(exp, distanct, typeof(T), typeof(T1), typeof(T2));
            return this;
        }

        private void HandleSelect(LambdaExpression exp, bool distanct, params Type[] types)
        {
            var expKey = makeKeyString(exp.Body, types);
            //var part = BaseFragment.GetPart<SelectFragment>(expKey, distanct);//new SelectFragment(distanct);//
            var part = new SelectFragment(distanct);
            part.SetTables<T>(ref tables, type);
            part.ResolveSql(exp.Body, types);
            Context.AddFragment(part);
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
            var expKey = makeKeyString(exp.Body, typeof(JoinTable));
            //JoinFragment part = BaseFragment.GetPart<JoinFragment>(expKey, joinType, typeof(JoinTable));//new JoinFragment(JoinType.Inner, typeof(JoinTable));//
            var part = new JoinFragment(joinType, typeof(JoinTable));
            part.SetTables<T>(ref tables, type);
            part.ResolveSql(exp.Body);
            Context.AddFragment(part);
        }

        #endregion

        #region Where

        public ExpressionSqlCore<T> Where(Expression<Func<T, bool>> exp)
        {
            HandleWherePart(exp, typeof(T));
            return this;
        }

        public ExpressionSqlCore<T> Where<T1>(Expression<Func<T, T1, bool>> exp)
        {
            HandleWherePart(exp, typeof(T), typeof(T1));
            return this;
        }
        public ExpressionSqlCore<T> Where<T1>(Expression<Func<T1, bool>> exp)
        {
            HandleWherePart(exp, typeof(T1));
            return this;
        }

        public ExpressionSqlCore<T> Where<T1, T2>(Expression<Func<T, T1, T2, bool>> exp)
        {
            HandleWherePart(exp, typeof(T), typeof(T1), typeof(T2));
            return this;
        }
        public ExpressionSqlCore<T> Where<T1, T2>(Expression<Func<T1, T2, bool>> exp)
        {
            HandleWherePart(exp, typeof(T1), typeof(T2));
            return this;
        }

        public ExpressionSqlCore<T> Where<T1, T2, T3>(Expression<Func<T, T1, T2, T3, bool>> exp)
        {
            HandleWherePart(exp, typeof(T), typeof(T1), typeof(T2), typeof(T3));
            return this;
        }
        public ExpressionSqlCore<T> Where<T1, T2, T3>(Expression<Func<T1, T2, T3, bool>> exp)
        {
            HandleWherePart(exp, typeof(T1), typeof(T2), typeof(T3));
            return this;
        }
        private void HandleWherePart(LambdaExpression exp, params Type[] types)
        {
            var expKey = makeKeyString(exp.Body, types);
            //WhereFragment part = BaseFragment.GetPart<WhereFragment>(expKey);//new WhereFragment();
            var part = new WhereFragment();
            part.SetTables<T>(ref tables, type);
            part.ResolveSql(exp.Body);
            part.ResolveParam(exp.Body);
            Context.AddFragment(part);
        }
        #endregion

        string makeKeyString(Expression exp, params Type[] types)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append($"{types[0].Name}_");
            for (int i = 1; i < types.Length; i++)
            {
                sb.Append($"{types[i].Name}_");
            }
            sb.Append($"{exp}");
            //Console.WriteLine(RemoveAlias(sb));
            return RemoveAlias(sb);
        }

        static string RemoveAlias(StringBuilder sb)
        {
            // 移除 lambda 中的参数名   // (?<!\.)
            return Regex.Replace(sb.ToString(), @"(\b\w{1,3})(?=\.)\.", "");
        }
    }
}

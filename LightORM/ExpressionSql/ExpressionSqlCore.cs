using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using System.Text.RegularExpressions;

namespace MDbContext.NewExpSql
{
    public abstract class ExpressionSqlCore { }
    public partial class ExpressionSqlCore<T>: ExpressionSqlCore
    {
        private readonly DbBaseType type;
        private ITableContext tableContext;
        private SqlContext sqlContext;
        List<(SqlPartial, string)> sqls;
        public ExpressionSqlCore(DbBaseType type, params Type[] tables)
        {
            //this.type = type;
            //tableContext = new TableContext<T>(type);
            //tableContext.SetTableAlias(typeof(T));
            //foreach (var t in tables)
            //{
            //    tableContext.SetTableAlias(t);
            //}
            //sqlContext = new SqlContext(tableContext);
            //sqls = new List<(SqlPartial, string)>();
        }

        #region Select
        public ExpressionSqlCore<T> Select(bool distanct)
        {
            SelectHandle(distanct);
            return this;
        }       

        #endregion

        #region Join
        public ExpressionSqlCore<T> InnerJoin<T1>(Expression<Func<T, T1, object>> exp)
        {
            JoinHandle("\n INNER", exp.Body);
            return this;
        }

        public ExpressionSqlCore<T> LeftJoin<T1>(Expression<Func<T, T1, object>> exp)
        {
            JoinHandle("\n LEFT", exp.Body);
            return this;
        }

        public ExpressionSqlCore<T> RightJoin<T1>(Expression<Func<T, T1, object>> exp)
        {
            JoinHandle("\n RIGHT", exp.Body);
            return this;
        }

        public ExpressionSqlCore<T> InnerJoin<T1, T2>(Expression<Func<T, T1, T2, object>> exp)
        {
            JoinHandle("\n INNER", exp.Body);
            return this;
        }

        public ExpressionSqlCore<T> InnerJoin<T1, T2>(Expression<Func<T1, T2, object>> exp)
        {
            JoinHandle("\n INNER", exp.Body);
            return this;
        }

        public ExpressionSqlCore<T> LeftJoin<T1, T2>(Expression<Func<T, T1, T2, object>> exp)
        {
            JoinHandle("\n LEFT", exp.Body);
            return this;
        }

        public ExpressionSqlCore<T> LeftJoin<T1, T2>(Expression<Func<T1, T2, object>> exp)
        {
            JoinHandle("\n LEFT", exp.Body);
            return this;
        }

        public ExpressionSqlCore<T> RightJoin<T1, T2>(Expression<Func<T, T1, T2, object>> exp)
        {
            JoinHandle("\n RIGHT", exp.Body);
            return this;
        }

        public ExpressionSqlCore<T> RightJoin<T1, T2>(Expression<Func<T1, T2, object>> exp)
        {
            JoinHandle("\n RIGHT", exp.Body);
            return this;
        }        

        #endregion

        #region Update
        public ExpressionSqlCore<T> Update(Expression<Func<object>> exp, Expression<Func<T, object>> ignoreExp = null)
        {
            UpdateHandle(exp.Body, ignoreExp?.Body);
            return this;
        }
       
        #endregion

        #region Insert
        public ExpressionSqlCore<T> Insert(Expression<Func<object>> exp)
        {
            InsertHandle(exp.Body);
            return this;
        }
        
        #endregion

        #region Where
        public ExpressionSqlCore<T> Where(Expression<Func<T, bool>> exp)
        {
            WhereHandle(exp.Body);
            return this;
        }

        public ExpressionSqlCore<T> Where<T1>(Expression<Func<T, T1, bool>> exp)
        {
            WhereHandle(exp.Body);
            return this;
        }
        public ExpressionSqlCore<T> Where<T1>(Expression<Func<T1, bool>> exp)
        {
            WhereHandle(exp.Body);
            return this;
        }

        public ExpressionSqlCore<T> Where<T1, T2>(Expression<Func<T, T1, T2, bool>> exp)
        {
            WhereHandle(exp.Body);
            return this;
        }
        public ExpressionSqlCore<T> Where<T1, T2>(Expression<Func<T1, T2, bool>> exp)
        {
            WhereHandle(exp.Body);
            return this;
        }

        public ExpressionSqlCore<T> Where<T1, T2, T3>(Expression<Func<T, T1, T2, T3, bool>> exp)
        {
            WhereHandle(exp.Body);
            return this;
        }
        public ExpressionSqlCore<T> Where<T1, T2, T3>(Expression<Func<T1, T2, T3, bool>> exp)
        {
            WhereHandle(exp.Body);
            return this;
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

using MDbContext;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace DExpSql
{
    public class ExpressionSql
    {

        private DbContext Context;
        private DbBaseType DbType { get; set; }
        internal SqlCaluse SqlCaluse { get; private set; }
        public ExpressionSql(DbBaseType dBType, DbContext context)
        {
            DbType = dBType;
            SqlCaluse = new SqlCaluse();
            SqlCaluse.DbType = DbType;
            this.Context = context;
        }
        /// <summary>
        /// Select
        /// </summary>
        /// <typeparam name="T">表1</typeparam>
        /// <param name="exp">列</param>
        /// <returns></returns>
        public ExpressionSqlCore<T> Select<T>(Expression<Func<T, object>> exp = null, bool distinct = false)
        {
            SqlCaluse.Clear();
            return new ExpressionSqlCore<T>(SqlCaluse, Context).Select(exp, distinct);
        }
        /// <summary>
        /// Select 
        /// </summary>
        /// <typeparam name="T">表1</typeparam>
        /// <typeparam name="T1">表2</typeparam>
        /// <param name="exp">列</param>
        /// <returns></returns>
        public ExpressionSqlCore<T> Select<T, T1>(Expression<Func<T, T1, object>> exp = null, bool distinct = false)
        {
            SqlCaluse.Clear();
            return new ExpressionSqlCore<T>(SqlCaluse, Context).Select(exp, distinct);
        }

        public ExpressionSqlCore<T> Select<T, T1, T2>(Expression<Func<T, T1, T2, object>> exp = null, bool distinct = false)
        {
            SqlCaluse.Clear();
            return new ExpressionSqlCore<T>(SqlCaluse, Context).Select(exp, distinct);
        }

        public ExpressionSqlCore<T> Select<T, T1, T2, T3>(Expression<Func<T, T1, T2, T3, object>> exp = null, bool distinct = false)
        {
            SqlCaluse.Clear();
            return new ExpressionSqlCore<T>(SqlCaluse, Context).Select(exp, distinct);
        }
        public ExpressionSqlCore<T> Select<T, T1, T2, T3, T4>(Expression<Func<T, T1, T2, T3, T4, object>> exp = null, bool distinct = false)
        {
            SqlCaluse.Clear();
            return new ExpressionSqlCore<T>(SqlCaluse, Context).Select(exp, distinct);
        }
        public ExpressionSqlCore<T> Select<T, T1, T2, T3, T4, T5>(Expression<Func<T, T1, T2, T3, T4, T5, object>> exp = null, bool distinct = false)
        {
            SqlCaluse.Clear();
            return new ExpressionSqlCore<T>(SqlCaluse, Context).Select(exp, distinct);
        }
        public ExpressionSqlCore<T> Select<T, T1, T2, T3, T4, T5, T6>(Expression<Func<T, T1, T2, T3, T4, T5, T6, object>> exp = null, bool distinct = false)
        {
            SqlCaluse.Clear();
            return new ExpressionSqlCore<T>(SqlCaluse, Context).Select(exp, distinct);
        }
        public ExpressionSqlCore<T> Select<T, T1, T2, T3, T4, T5, T6, T7>(Expression<Func<T, T1, T2, T3, T4, T5, T6, T7, object>> exp = null, bool distinct = false)
        {
            SqlCaluse.Clear();
            return new ExpressionSqlCore<T>(SqlCaluse, Context).Select(exp, distinct);
        }

        public ExpressionSqlCore<T> Select<T, T1, T2, T3, T4, T5, T6, T7, T8>(Expression<Func<T, T1, T2, T3, T4, T5, T6, T7, T8, object>> exp = null, bool distinct = false)
        {
            SqlCaluse.Clear();
            return new ExpressionSqlCore<T>(SqlCaluse, Context).Select(exp, distinct);
        }
        public ExpressionSqlCore<T> Select<T, T1, T2, T3, T4, T5, T6, T7, T8, T9>(Expression<Func<T, T1, T2, T3, T4, T5, T6, T7, T8, T9, object>> exp = null, bool distinct = false)
        {
            SqlCaluse.Clear();
            return new ExpressionSqlCore<T>(SqlCaluse, Context).Select(exp, distinct);
        }

        public ExpressionSqlCore<T> Update<T>(Expression<Func<object>> exp, Expression<Func<T, object>> pkExp = null)
        {
            SqlCaluse.Clear();
            return new ExpressionSqlCore<T>(SqlCaluse, Context).Update(exp, pkExp);
        }

        /// <summary>
        /// 更新实体
        /// </summary>
        /// <typeparam name="T">更新的实体类型</typeparam>
        /// <param name="entity">实体实例</param>
        /// <param name="ingore">忽略的列</param>
        /// <returns></returns>
        public ExpressionSqlCore<T> Update<T>(T entity, Expression<Func<T, object>> ingore = null)
        {
            return Update<T>(() => entity, ingore);
        }

        public ExpressionSqlCore<T> Insert<T>(Expression<Func<object>> exp)
        {
            SqlCaluse.Clear();
            return new ExpressionSqlCore<T>(SqlCaluse, Context).Insert(exp);
        }

        public ExpressionSqlCore<T> Insert<T>(T entity)
        {
            return Insert<T>(() => entity);
        }

        public ExpressionSqlCore<T> Delete<T>()
        {
            SqlCaluse.Clear();
            return new ExpressionSqlCore<T>(SqlCaluse, Context).Delete();
        }

        public ExpressionSqlCore<T> Count<T>()
        {
            SqlCaluse.Clear();
            return new ExpressionSqlCore<T>(SqlCaluse, Context).Count();
        }

        public ExpressionSqlCore<T> Max<T>(Expression<Func<T, object>> exp)
        {
            SqlCaluse.Clear();
            return new ExpressionSqlCore<T>(SqlCaluse, Context).Max(exp);
        }

        public override string ToString()
        {
            return SqlCaluse.Sql.ToString() + "\n\n" + paramString();
        }

        private string paramString()
        {
            StringBuilder sb = new StringBuilder();
            foreach (KeyValuePair<string, object> item in SqlCaluse.SqlParam)
            {
                sb.AppendLine($"[{item.Key},{item.Value}]");
            }
            return sb.ToString();
        }

    }
}

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace DExpSql {
    public class ExpressionSql {
        private int DbType { get; set; }
        public SqlCaluse SqlCaluse { get; private set; }
        public ExpressionSql(int dBType) {
            DbType = dBType;
            SqlCaluse = new SqlCaluse();
            SqlCaluse.DbType = DbType;
        }
        /// <summary>
        /// Select
        /// </summary>
        /// <typeparam name="T">表1</typeparam>
        /// <param name="exp">列</param>
        /// <returns></returns>
        public ExpressionSqlCore<T> Select<T>(Expression<Func<T, object>> exp = null) {
            SqlCaluse.Clear();
            return new ExpressionSqlCore<T>(SqlCaluse).Select(exp);
        }
        /// <summary>
        /// Select 
        /// </summary>
        /// <typeparam name="T">表1</typeparam>
        /// <typeparam name="T1">表2</typeparam>
        /// <param name="exp">列</param>
        /// <returns></returns>
        public ExpressionSqlCore<T> Select<T, T1>(Expression<Func<T, T1, object>> exp = null) {
            SqlCaluse.Clear();
            return new ExpressionSqlCore<T>(SqlCaluse).Select(exp);
        }

        public ExpressionSqlCore<T> Select<T, T1, T2>(Expression<Func<T, T1, T2, object>> exp = null) {
            SqlCaluse.Clear();
            return new ExpressionSqlCore<T>(SqlCaluse).Select(exp);
        }

        public ExpressionSqlCore<T> Select<T, T1, T2, T3>(Expression<Func<T, T1, T2, T3, object>> exp = null) {
            SqlCaluse.Clear();
            return new ExpressionSqlCore<T>(SqlCaluse).Select(exp);
        }

        public ExpressionSqlCore<T> Update<T>(Expression<Func<object>> exp, Expression<Func<T, object>> pkExp = null) {
            SqlCaluse.Clear();
            return new ExpressionSqlCore<T>(SqlCaluse).Update(exp, pkExp);
        }

        /// <summary>
        /// 更新实体
        /// </summary>
        /// <typeparam name="T">更新的实体类型</typeparam>
        /// <param name="entity">实体实例</param>
        /// <param name="pkExp">忽略的列</param>
        /// <returns></returns>
        public ExpressionSqlCore<T> Update<T>(T entity, Expression<Func<T, object>> pkExp = null) {
            return Update<T>(() => entity, pkExp);
        }

        public ExpressionSqlCore<T> Insert<T>(Expression<Func<object>> exp) {
            SqlCaluse.Clear();
            return new ExpressionSqlCore<T>(SqlCaluse).Insert(exp);
        }

        public ExpressionSqlCore<T> Insert<T>(T entity) {
            return Insert<T>(() => entity);
        }

        public ExpressionSqlCore<T> Delete<T>() {
            SqlCaluse.Clear();
            return new ExpressionSqlCore<T>(SqlCaluse).Delete();
        }

        public ExpressionSqlCore<T> Count<T>() {
            SqlCaluse.Clear();
            return new ExpressionSqlCore<T>(SqlCaluse).Count();
        }

        public ExpressionSqlCore<T> Max<T>(string col) {
            SqlCaluse.Clear();
            return new ExpressionSqlCore<T>(SqlCaluse).Max(col);
        }

        public override string ToString() {
            return SqlCaluse.Sql.ToString() + "\n\n" + paramString();
        }

        private string paramString() {
            StringBuilder sb = new StringBuilder();
            foreach (KeyValuePair<string, object> item in SqlCaluse.SqlParam) {
                sb.AppendLine($"[{item.Key},{item.Value}]");
            }
            return sb.ToString();
        }

    }
}

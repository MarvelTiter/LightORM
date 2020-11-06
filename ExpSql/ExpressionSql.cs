using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace DExpSql
{
    public class ExpressionSql
    {
        private int DbType { get; set; }
        public SqlCaluse SqlCaluse { get; private set; }
        public ExpressionSql(int dBType)
        {
            DbType = dBType;
            SqlCaluse = new SqlCaluse();
            SqlCaluse.DbType = DbType;
        }

        public ExpressionSqlCore<T> Select<T>(Expression<Func<T, object>> exp = null)
        {
            SqlCaluse.Clear();
            return new ExpressionSqlCore<T>(SqlCaluse).Select(exp);
        }

        public ExpressionSqlCore<T> Select<T, T1>(Expression<Func<T, T1, object>> exp = null)
        {
            SqlCaluse.Clear();
            return new ExpressionSqlCore<T>(SqlCaluse).Select(exp);
        }

        public ExpressionSqlCore<T> Update<T>(Expression<Func<object>> exp, Expression<Func<T, object>> pkExp = null)
        {
            SqlCaluse.Clear();
            return new ExpressionSqlCore<T>(SqlCaluse).Update(exp, pkExp);
        }

        public ExpressionSqlCore<T> Update<T>(T entity, Expression<Func<T,object>> pkExp = null)
        {
            return Update<T>(() => entity, pkExp);
        }

        public ExpressionSqlCore<T> Insert<T>(Expression<Func<object>> exp)
        {
            SqlCaluse.Clear();
            return new ExpressionSqlCore<T>(SqlCaluse).Insert(exp);
        }

        public ExpressionSqlCore<T> Insert<T>(T entity)
        {
            return Insert<T>(() => entity);
        }

        public ExpressionSqlCore<T> Delete<T>()
        {
            SqlCaluse.Clear();
            return new ExpressionSqlCore<T>(SqlCaluse).Delete();
        }

        public ExpressionSqlCore<T> Count<T>()
        {
            SqlCaluse.Clear();
            return new ExpressionSqlCore<T>(SqlCaluse).Count();
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

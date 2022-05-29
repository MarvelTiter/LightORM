using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace MDbContext.NewExpSql
{
    public class ExpressionSql
    {
        private readonly DbBaseType dbType;

        public ExpressionSql(DbBaseType dbType)
        {
            this.dbType = dbType;
        }
        ExpressionSqlCore sqlCore;

        public ExpressionSqlCore<T> Select<T>(Expression<Func<T, object>> exp = null, bool distanct = false)
        {
            sqlCore = new ExpressionSqlCore<T>(dbType).Select(distanct);
            return (ExpressionSqlCore<T>)sqlCore;
        }

        public ExpressionSqlCore<T> Select<T, T1>(Expression<Func<T, T1, object>> exp = null, bool distanct = false)
        {
            sqlCore = new ExpressionSqlCore<T>(dbType, typeof(T1)).Select(distanct);
            return (ExpressionSqlCore<T>)sqlCore;
        }

        public ExpressionSqlCore<T> Update<T>(Expression<Func<object>> exp, Expression<Func<T, object>> ignore = null)
        {
            sqlCore = new ExpressionSqlCore<T>(dbType).Update(exp, ignore);
            return (ExpressionSqlCore<T>)sqlCore;
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
            sqlCore = new ExpressionSqlCore<T>(dbType).Insert(exp);            
            return (ExpressionSqlCore<T>)sqlCore;
        }

        public ExpressionSqlCore<T> Insert<T>(T entity)
        {
            sqlCore = Insert<T>(() => entity);
            return (ExpressionSqlCore<T>)sqlCore;
        }

#if DEBUG
        public override string ToString()
        {
            return sqlCore.ToString();
        }
#endif
    }
}

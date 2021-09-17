using System;
using System.ComponentModel;
using System.Linq.Expressions;

namespace DExpSql
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public partial class ExpressionSqlCore<T>
    {
        private SqlCaluse _sqlCaluse;
        public ExpressionSqlCore(SqlCaluse sqlCaluse)
        {
            this._sqlCaluse = sqlCaluse;
        }

        #region select part

        public ExpressionSqlCore<T> Select(Expression<Func<T, object>> exp, bool distinct)
        {
            if (exp == null)
            {
                _sqlCaluse.SelectAll = true;
                exp = t => t;
            }
            SelectHandle(distinct, exp.Body, typeof(T));
            return this;
        }

        public ExpressionSqlCore<T> Select<T1>(Expression<Func<T, T1, object>> exp, bool distinct)
        {
            if (exp == null)
            {
                _sqlCaluse.SelectAll = true;
                exp = (t1, t2) => new { t1, t2 };
            }
            SelectHandle(distinct, exp.Body, typeof(T), typeof(T1));
            return this;
        }

        public ExpressionSqlCore<T> Select<T1, T2>(Expression<Func<T, T1, T2, object>> exp, bool distinct)
        {
            if (exp == null)
            {
                _sqlCaluse.SelectAll = true;
                exp = (t1, t2, t3) => new { t1, t2, t3 };
            }
            SelectHandle(distinct, exp.Body, typeof(T), typeof(T1), typeof(T2));
            return this;
        }

        public ExpressionSqlCore<T> Select<T1, T2, T3>(Expression<Func<T, T1, T2, T3, object>> exp, bool distinct)
        {
            if (exp == null)
            {
                _sqlCaluse.SelectAll = true;
                exp = (t1, t2, t3, t4) => new { t1, t2, t3, t4 };
            }
            SelectHandle(distinct, exp.Body, typeof(T), typeof(T1), typeof(T2), typeof(T3));
            return this;
        }
        #endregion

        #region update part
        public ExpressionSqlCore<T> Update(Expression<Func<object>> exp, Expression<Func<T, object>> pkExp = null)
        {
            UpdateHandle(exp.Body, pkExp?.Body);
            return this;
        }
        #endregion

        #region insert part
        public ExpressionSqlCore<T> Insert(Expression<Func<object>> exp)
        {
            var sql = InsertHandle();
            ExpressionVisit.Insert(exp.Body, _sqlCaluse);
            _sqlCaluse.Sql.AppendFormat(sql, _sqlCaluse.SelectedFieldString, _sqlCaluse.ParamString);
            return this;
        }
        #endregion

        #region delete part
        public ExpressionSqlCore<T> Delete()
        {
            var sql = DeleteHandle();
            _sqlCaluse += sql;
            return this;
        }
        #endregion

        #region join part
        public ExpressionSqlCore<T> InnerJoin<T1>(Expression<Func<T, T1, object>> exp)
        {
            JoinHandle<T1>("\n INNER", exp.Body);
            return this;
        }

        public ExpressionSqlCore<T> LeftJoin<T1>(Expression<Func<T, T1, object>> exp)
        {
            JoinHandle<T1>("\n LEFT", exp.Body);
            return this;
        }

        public ExpressionSqlCore<T> RightJoin<T1>(Expression<Func<T, T1, object>> exp)
        {
            JoinHandle<T1>("\n RIGHT", exp.Body);
            return this;
        }

        public ExpressionSqlCore<T> InnerJoin<T1, T2>(Expression<Func<T, T1, T2, object>> exp)
        {
            JoinHandle<T1>("\n INNER", exp.Body);
            return this;
        }

        public ExpressionSqlCore<T> InnerJoin<T1, T2>(Expression<Func<T1, T2, object>> exp)
        {
            JoinHandle<T1>("\n INNER", exp.Body);
            return this;
        }

        public ExpressionSqlCore<T> LeftJoin<T1, T2>(Expression<Func<T, T1, T2, object>> exp)
        {
            JoinHandle<T1>("\n LEFT", exp.Body);
            return this;
        }

        public ExpressionSqlCore<T> LeftJoin<T1, T2>(Expression<Func<T1, T2, object>> exp)
        {
            JoinHandle<T1>("\n LEFT", exp.Body);
            return this;
        }

        public ExpressionSqlCore<T> RightJoin<T1, T2>(Expression<Func<T, T1, T2, object>> exp)
        {
            JoinHandle<T1>("\n RIGHT", exp.Body);
            return this;
        }

        public ExpressionSqlCore<T> RightJoin<T1, T2>(Expression<Func<T1, T2, object>> exp)
        {
            JoinHandle<T1>("\n RIGHT", exp.Body);
            return this;
        }

        #endregion

        #region where part
        public ExpressionSqlCore<T> Where(Expression<Func<T, object>> exp)
        {
            WhereHandle(exp.Body);
            return this;
        }

        public ExpressionSqlCore<T> Where<T1>(Expression<Func<T, T1, object>> exp)
        {
            WhereHandle(exp.Body);
            return this;
        }
        public ExpressionSqlCore<T> Where<T1>(Expression<Func<T1, object>> exp)
        {
            WhereHandle(exp.Body);
            return this;
        }

        public ExpressionSqlCore<T> Where<T1, T2>(Expression<Func<T, T1, T2, object>> exp)
        {
            WhereHandle(exp.Body);
            return this;
        }
        public ExpressionSqlCore<T> Where<T1, T2>(Expression<Func<T1, T2, object>> exp)
        {
            WhereHandle(exp.Body);
            return this;
        }

        public ExpressionSqlCore<T> Where<T1, T2, T3>(Expression<Func<T, T1, T2, T3, object>> exp)
        {
            WhereHandle(exp.Body);
            return this;
        }
        public ExpressionSqlCore<T> Where<T1, T2, T3>(Expression<Func<T1, T2, T3, object>> exp)
        {
            WhereHandle(exp.Body);
            return this;
        }
        #endregion

        #region IfWhere part
        public ExpressionSqlCore<T> IfWhere(Func<bool> condition, Expression<Func<T, object>> exp)
        {
            if (condition.Invoke())
                WhereHandle(exp.Body);
            return this;
        }

        public ExpressionSqlCore<T> IfWhere<T1>(Func<bool> condition, Expression<Func<T, T1, object>> exp)
        {
            if (condition.Invoke())
                WhereHandle(exp.Body);
            return this;
        }
        public ExpressionSqlCore<T> IfWhere<T1>(Func<bool> condition, Expression<Func<T1, object>> exp)
        {
            if (condition.Invoke())
                WhereHandle(exp.Body);
            return this;
        }

        public ExpressionSqlCore<T> IfWhere<T1, T2>(Func<bool> condition, Expression<Func<T, T1, T2, object>> exp)
        {
            if (condition.Invoke())
                WhereHandle(exp.Body);
            return this;
        }
        public ExpressionSqlCore<T> IfWhere<T1, T2>(Func<bool> condition, Expression<Func<T1, T2, object>> exp)
        {
            if (condition.Invoke())
                WhereHandle(exp.Body);
            return this;
        }

        public ExpressionSqlCore<T> IfWhere<T1, T2, T3>(Func<bool> condition, Expression<Func<T, T1, T2, T3, object>> exp)
        {
            if (condition.Invoke())
                WhereHandle(exp.Body);
            return this;
        }
        public ExpressionSqlCore<T> IfWhere<T1, T2, T3>(Func<bool> condition, Expression<Func<T1, T2, T3, object>> exp)
        {
            if (condition.Invoke())
                WhereHandle(exp.Body);
            return this;
        }
        #endregion

        #region group by
        public ExpressionSqlCore<T> GroupBy(Expression<Func<T, object>> exp)
        {
            GroupByHandle(exp.Body);
            return this;
        }

        public ExpressionSqlCore<T> GroupBy<T1>(Expression<Func<T, T1, object>> exp)
        {
            GroupByHandle(exp.Body);
            return this;
        }

        public ExpressionSqlCore<T> GroupBy<T1>(Expression<Func<T1, object>> exp)
        {
            GroupByHandle(exp.Body);
            return this;
        }

        public ExpressionSqlCore<T> GroupBy<T1, T2>(Expression<Func<T, T1, T2, object>> exp)
        {
            GroupByHandle(exp.Body);
            return this;
        }

        public ExpressionSqlCore<T> GroupBy<T1, T2>(Expression<Func<T1, T2, object>> exp)
        {
            GroupByHandle(exp.Body);
            return this;
        }

        public ExpressionSqlCore<T> GroupBy<T1, T2, T3>(Expression<Func<T, T1, T2, T3, object>> exp)
        {
            GroupByHandle(exp.Body);
            return this;
        }

        public ExpressionSqlCore<T> GroupBy<T1, T2, T3>(Expression<Func<T1, T2, T3, object>> exp)
        {
            GroupByHandle(exp.Body);
            return this;
        }

        #endregion

        #region order by asc
        public ExpressionSqlCore<T> OrderByAsc(Expression<Func<T, object>> exp)
        {
            OrderByHandle(exp.Body);
            _sqlCaluse += " ASC";
            return this;
        }

        public ExpressionSqlCore<T> OrderByAsc<T1>(Expression<Func<T, T1, object>> exp)
        {
            OrderByHandle(exp.Body);
            _sqlCaluse += " ASC";
            return this;
        }
        public ExpressionSqlCore<T> OrderByAsc<T1>(Expression<Func<T1, object>> exp)
        {
            OrderByHandle(exp.Body);
            _sqlCaluse += " ASC";
            return this;
        }
        public ExpressionSqlCore<T> OrderByAsc<T1, T2>(Expression<Func<T, T1, T2, object>> exp)
        {
            OrderByHandle(exp.Body);
            _sqlCaluse += " ASC";
            return this;
        }
        public ExpressionSqlCore<T> OrderByAsc<T1, T2>(Expression<Func<T1, T2, object>> exp)
        {
            OrderByHandle(exp.Body);
            _sqlCaluse += " ASC";
            return this;
        }

        public ExpressionSqlCore<T> OrderByAsc<T1, T2, T3>(Expression<Func<T, T1, T2, T3, object>> exp)
        {
            OrderByHandle(exp.Body);
            _sqlCaluse += " ASC";
            return this;
        }
        public ExpressionSqlCore<T> OrderByAsc<T1, T2, T3>(Expression<Func<T1, T2, T3, object>> exp)
        {
            OrderByHandle(exp.Body);
            _sqlCaluse += " ASC";
            return this;
        }
        #endregion

        #region order by desc
        public ExpressionSqlCore<T> OrderByDesc(Expression<Func<T, object>> exp)
        {
            OrderByHandle(exp.Body);
            _sqlCaluse += " DESC";
            return this;
        }
        public ExpressionSqlCore<T> OrderByDesc<T1>(Expression<Func<T, T1, object>> exp)
        {
            OrderByHandle(exp.Body);
            _sqlCaluse += " DESC";
            return this;
        }
        public ExpressionSqlCore<T> OrderByDesc<T1>(Expression<Func<T1, object>> exp)
        {
            OrderByHandle(exp.Body);
            _sqlCaluse += " DESC";
            return this;
        }
        public ExpressionSqlCore<T> OrderByDesc<T1, T2>(Expression<Func<T, T1, T2, object>> exp)
        {
            OrderByHandle(exp.Body);
            _sqlCaluse += " DESC";
            return this;
        }
        public ExpressionSqlCore<T> OrderByDesc<T1, T2>(Expression<Func<T1, T2, object>> exp)
        {
            OrderByHandle(exp.Body);
            _sqlCaluse += " DESC";
            return this;
        }
        public ExpressionSqlCore<T> OrderByDesc<T1, T2, T3>(Expression<Func<T, T1, T2, T3, object>> exp)
        {
            OrderByHandle(exp.Body);
            _sqlCaluse += " DESC";
            return this;
        }
        public ExpressionSqlCore<T> OrderByDesc<T1, T2, T3>(Expression<Func<T1, T2, T3, object>> exp)
        {
            OrderByHandle(exp.Body);
            _sqlCaluse += " DESC";
            return this;
        }
        #endregion

        public ExpressionSqlCore<T> Paging(int from, int to)
        {
            _sqlCaluse.Paging(from, to);
            return this;
        }

        public ExpressionSqlCore<T> Count()
        {
            CountHandle();
            return this;
        }

        public ExpressionSqlCore<T> Max(Expression<Func<T, object>> exp)
        {
            MaxHandle(exp.Body);
            return this;
        }

        public ExpressionSqlCore<T> Top(int count)
        {

            return this;
        }

    }
}

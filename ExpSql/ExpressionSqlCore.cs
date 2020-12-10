using System;
using System.ComponentModel;
using System.Linq.Expressions;

namespace DExpSql {
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public partial class ExpressionSqlCore<T> {
        private SqlCaluse _sqlCaluse;
        public ExpressionSqlCore(SqlCaluse sqlCaluse) {
            this._sqlCaluse = sqlCaluse;
        }
        #region select part
       
        public ExpressionSqlCore<T> Select(Expression<Func<T, object>> exp) {
            if (exp == null) {
                _sqlCaluse.SelectAll = true;
                exp = t => t;
            }
            SelectHandle(exp.Body, typeof(T));
            //ExpressionVisit.Select(exp.Body, _sqlCaluse);
            //var selected = _sqlCaluse.SelectAll ? "*" : _sqlCaluse.SelectedFieldString;
            //_sqlCaluse.Sql.AppendFormat(sql, selected);
            return this;
        }

        public ExpressionSqlCore<T> Select<T1>(Expression<Func<T, T1, object>> exp = null) {
            if (exp == null) {
                _sqlCaluse.SelectAll = true;
                exp = (t1, t2) => new { t1, t2 };
            }
            SelectHandle(exp.Body, typeof(T), typeof(T1));

            return this;
        }

        public ExpressionSqlCore<T> Select<T1, T2>(Expression<Func<T, T1, T2, object>> exp = null) {
            if (exp == null) {
                _sqlCaluse.SelectAll = true;
                exp = (t1, t2, t3) => new { t1, t2, t3 };
            }
            SelectHandle(exp.Body, typeof(T), typeof(T1), typeof(T2));
            return this;
        }

        public ExpressionSqlCore<T> Select<T1, T2, T3>(Expression<Func<T, T1, T2, T3, object>> exp = null) {
            if (exp == null) {
                _sqlCaluse.SelectAll = true;
                exp = (t1, t2, t3, t4) => new { t1, t2, t3, t4 };
            }
            SelectHandle(exp.Body, typeof(T), typeof(T1), typeof(T2), typeof(T3));
            return this;
        }
        #endregion

        #region update part
        /// <summary>
        /// 更新实体
        /// </summary>
        /// <param name="exp"></param>
        /// <param name="pkExp">忽略的列</param>
        /// <returns></returns>
        public ExpressionSqlCore<T> Update(Expression<Func<object>> exp, Expression<Func<T, object>> pkExp = null) {
            UpdateHandle(exp, pkExp);
            return this;
        }
        #endregion

        #region insert part
        public ExpressionSqlCore<T> Insert(Expression<Func<object>> exp) {
            var sql = InsertHandle();
            ExpressionVisit.Insert(exp.Body, _sqlCaluse);
            _sqlCaluse.Sql.AppendFormat(sql, _sqlCaluse.SelectedFieldString, _sqlCaluse.ParamString);
            return this;
        }
        #endregion

        #region delete part
        public ExpressionSqlCore<T> Delete() {
            var sql = DeleteHandle();
            _sqlCaluse += sql;
            return this;
        }
        #endregion

        #region join part
        public ExpressionSqlCore<T> InnerJoin<T1>(Expression<Func<T, T1, object>> exp) {
            JoinHandle("\n INNER", exp);
            return this;
        }

        public ExpressionSqlCore<T> LeftJoin<T1>(Expression<Func<T, T1, object>> exp) {
            JoinHandle("\n LEFT", exp);
            return this;
        }

        public ExpressionSqlCore<T> RightJoin<T1>(Expression<Func<T, T1, object>> exp) {
            JoinHandle("\n RIGHT", exp);
            return this;
        }
        #endregion

        #region where part
        public ExpressionSqlCore<T> Where(Expression<Func<T, object>> exp) {
            WhereHandle(exp.Body);
            return this;
        }

        public ExpressionSqlCore<T> Where<T1>(Expression<Func<T, T1, object>> exp) {
            WhereHandle(exp.Body);
            return this;
        }

        public ExpressionSqlCore<T> Where<T1, T2>(Expression<Func<T, T1, T2, object>> exp) {
            WhereHandle(exp.Body);
            return this;
        }

        public ExpressionSqlCore<T> Where<T1, T2, T3>(Expression<Func<T, T1, T2, T3, object>> exp) {
            WhereHandle(exp.Body);
            return this;
        }
        #endregion 

        public ExpressionSqlCore<T> OrderByAsc(Expression<Func<T, object>> exp) {
            OrderByHandle(exp);
            _sqlCaluse += " ASC";
            return this;
        }

        public ExpressionSqlCore<T> OrderByDesc(Expression<Func<T, object>> exp) {
            OrderByHandle(exp);
            _sqlCaluse += " DESC";
            return this;
        }

        public ExpressionSqlCore<T> Paging(int from, int to) {
            _sqlCaluse.Paging(from, to);
            return this;
        }

        public ExpressionSqlCore<T> Count() {
            CountHandle();
            return this;
        }

        public ExpressionSqlCore<T> Max(string column) {
            MaxHandle(column);
            return this;
        }

    }
}

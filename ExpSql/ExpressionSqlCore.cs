using System;
using System.ComponentModel;
using System.Linq.Expressions;

namespace DExpSql
{
    public partial class ExpressionSqlCore<T>
    {
        private SqlCaluse _sqlCaluse;

        public ExpressionSqlCore(SqlCaluse sqlCaluse)
        {
            this._sqlCaluse = sqlCaluse;
        }

        public ExpressionSqlCore<T> Select(Expression<Func<T, object>> exp)
        {
            if (exp == null)
            {
                _sqlCaluse.SelectAll = true;
                exp = t => t;
            }

            var sql = SelectHandle(typeof(T));
            ExpressionVisit.Select(exp.Body, _sqlCaluse);
            var selected = _sqlCaluse.SelectAll ? "*" : _sqlCaluse.SelectedFieldString;
            _sqlCaluse.Sql.AppendFormat(sql, selected);
            return this;
        }

        public ExpressionSqlCore<T> Select<T1>(Expression<Func<T, T1, object>> exp = null)
        {
            var sql = SelectHandle(typeof(T), typeof(T1));
            if (exp == null)
            {
                _sqlCaluse.SelectAll = true;
                exp = (t1, t2) => new { t1, t2 };
            }
            ExpressionVisit.Select(exp.Body, _sqlCaluse);
            var selected = _sqlCaluse.SelectAll ? "*" : _sqlCaluse.SelectedFieldString;
            _sqlCaluse.Sql.AppendFormat(sql, selected);
            return this;
        }

        public ExpressionSqlCore<T> Update(Expression<Func<object>> exp, Expression<Func<T, object>> pkExp = null)
        {
            UpdateHandle(exp, pkExp);
            return this;
        }

        public ExpressionSqlCore<T> Insert(Expression<Func<object>> exp)
        {
            var sql = InsertHandle();
            ExpressionVisit.Insert(exp.Body, _sqlCaluse);
            _sqlCaluse.Sql.AppendFormat(sql, _sqlCaluse.SelectedFieldString, _sqlCaluse.ParamString);
            return this;
        }

        public ExpressionSqlCore<T> Delete()
        {
            var sql = DeleteHandle();
            _sqlCaluse += sql;
            return this;
        }

        public ExpressionSqlCore<T> InnerJoin<T1>(Expression<Func<T, T1, object>> exp)
        {
            JoinHandle("\n INNER", exp);
            return this;
        }

        public ExpressionSqlCore<T> LeftJoin<T1>(Expression<Func<T, T1, object>> exp)
        {
            JoinHandle("\n LEFT", exp);
            return this;
        }

        public ExpressionSqlCore<T> RightJoin<T1>(Expression<Func<T, T1, object>> exp)
        {
            JoinHandle("\n RIGHT", exp);
            return this;
        }

        public ExpressionSqlCore<T> Where(Expression<Func<T, object>> exp)
        {
            WhereHandle(exp);
            return this;
        }

        public ExpressionSqlCore<T> OrderByAsc(Expression<Func<T, object>> exp)
        {
            OrderByHandle(exp);
            _sqlCaluse += " ASC";
            return this;
        }

        public ExpressionSqlCore<T> OrderByDesc(Expression<Func<T, object>> exp)
        {
            OrderByHandle(exp);
            _sqlCaluse += " DESC";
            return this;
        }

        public ExpressionSqlCore<T> Paging(int from,int to)
        {
            _sqlCaluse.Paging(from, to);
            return this;
        }

        public ExpressionSqlCore<T> Count()
        {
            CountHandle();
            return this;
        }

    }
}

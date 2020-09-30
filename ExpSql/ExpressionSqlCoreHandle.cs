using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace DExpSql
{
    public partial class ExpressionSqlCore<T>
    {
        private bool _firstWhere;

        private string SelectHandle(params Type[] arr)
        {
            _sqlCaluse.Clear();
            var sql = " SELECT {0}\n FROM ";
            foreach (Type item in arr)
            {
                _sqlCaluse.SetTableAlias(item.Name);
            }
            return sql + typeof(T).Name + " " + _sqlCaluse.GetTableAlias(typeof(T).Name);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sqlCaluse"></param>
        /// <param name="joinType">inner left right</param>
        /// <returns></returns>
        private void JoinHandle<T1>(string joinType, Expression<Func<T, T1, object>> exp)
        {
            var tableName = typeof(T1).Name;
            _sqlCaluse.SetTableAlias(tableName);
            _sqlCaluse += $"{joinType} JOIN {tableName} {_sqlCaluse.GetTableAlias(tableName)}";
            ExpressionVisit.Join(exp.Body, _sqlCaluse);
        }

        private void WhereHandle(Expression<Func<T, object>> exp)
        {
            if (_firstWhere)
            {
                _sqlCaluse += "\n WHERE";
                _firstWhere = false;
            }
            else
                _sqlCaluse += "\n AND";
            ExpressionVisit.Where(exp.Body, _sqlCaluse);
        }

        private void UpdateHandle(Expression<Func<object>> exp, Expression<Func<T,object>> pkExp = null)
        {
            //var tableName = typeof(T).Name;
            _sqlCaluse += $" UPDATE {typeof(T).Name} SET \n";
            if (null != pkExp)
                ExpressionVisit.PrimaryKey(pkExp.Body, _sqlCaluse);
            ExpressionVisit.Update(exp.Body, _sqlCaluse);
        }

        private string InsertHandle()
        {
            return $" INSERT INTO {typeof(T).Name} ({{0}}) \n VALUES ({{1}})";
        }

        private void OrderByHandle(Expression<Func<T, object>> exp)
        {
            _sqlCaluse += "\n ORDER BY";
            ExpressionVisit.OrderBy(exp.Body, _sqlCaluse);
        }

        private void CountHandle()
        {
            var tbName = typeof(T).Name;
            _sqlCaluse.SetTableAlias(tbName);
            var alia = _sqlCaluse.GetTableAlias(tbName);
            _sqlCaluse += $" SELECT COUNT(*) FROM {tbName} {alia}";
        }
    }
}

using MDbEntity.Attributes;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace DExpSql {
    public partial class ExpressionSqlCore<T> {
        private bool _firstWhere = true;


        private void SelectHandle(Expression body, params Type[] arr) {
            _sqlCaluse.Clear();
            var sql = " SELECT {0}\n FROM ";
            foreach (Type item in arr) {
                _sqlCaluse.SetTableAlias(item);
            }
            var mainTable = typeof(T);
            var tableName = _sqlCaluse.GetTableName(mainTable);
            sql = sql + tableName + " " + _sqlCaluse.GetTableAlias(mainTable);
            ExpressionVisit.Select(body, _sqlCaluse);
            var selected = _sqlCaluse.SelectAll ? "*" : _sqlCaluse.SelectedFieldString;
            _sqlCaluse.Sql.AppendFormat(sql, selected);
        }

        private void JoinHandle<T1>(string joinType, Expression<Func<T, T1, object>> exp) {
            var joinTable = typeof(T1);
            _sqlCaluse.SetTableAlias(joinTable);
            var tableName = _sqlCaluse.GetTableName(joinTable);
            _sqlCaluse += $"{joinType} JOIN {tableName} {_sqlCaluse.GetTableAlias(joinTable)}";
            ExpressionVisit.Join(exp.Body, _sqlCaluse);
        }

        private void WhereHandle(Expression body) {
            if (_firstWhere) {
                _sqlCaluse += "\n WHERE";
                _firstWhere = false;
            } else
                _sqlCaluse += "\n AND";
            ExpressionVisit.Where(body, _sqlCaluse);
        }

        private void UpdateHandle(Expression<Func<object>> exp, Expression<Func<T, object>> pkExp = null) {
            var tableName = _sqlCaluse.GetTableName(typeof(T));
            _sqlCaluse += $" UPDATE {tableName} SET \n";
            if (null != pkExp)
                ExpressionVisit.PrimaryKey(pkExp.Body, _sqlCaluse);
            ExpressionVisit.Update(exp.Body, _sqlCaluse);
        }

        private string InsertHandle() {
            var tableName = _sqlCaluse.GetTableName(typeof(T));
            return $" INSERT INTO {tableName} ({{0}}) \n VALUES ({{1}})";
        }

        private string DeleteHandle() {
            var tableName = _sqlCaluse.GetTableName(typeof(T));
            return $" DELETE FROM {tableName} \n";
        }

        private void OrderByHandle(Expression<Func<T, object>> exp) {
            _sqlCaluse += "\n ORDER BY";
            ExpressionVisit.OrderBy(exp.Body, _sqlCaluse);
        }

        private void CountHandle() {
            var tbType = typeof(T);
            _sqlCaluse.SetTableAlias(tbType);
            var tbName = _sqlCaluse.GetTableName(tbType);
            var alia = _sqlCaluse.GetTableAlias(tbType);
            _sqlCaluse += $" SELECT COUNT(*) FROM {tbName} {alia}";
        }

        private void MaxHandle(string col) {
            var tbType = typeof(T);
            var tbName = _sqlCaluse.GetTableName(tbType);
            _sqlCaluse.SetTableAlias(tbType);
            var alia = _sqlCaluse.GetTableAlias(tbType);
            _sqlCaluse += $" SELECT MAX({col}) FROM {tbName} {alia}";
        }
    }
}

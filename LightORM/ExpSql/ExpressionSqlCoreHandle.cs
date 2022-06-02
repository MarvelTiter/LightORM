using MDbContext;
using MDbContext.ExpSql.Extension;
using MDbEntity.Attributes;
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
        private bool _firstWhere = true;
        private bool _firstOrderby = true;
        int selectContentIndexStart = 0;
        int selectContentIndexEnd = 0;
        private void SelectHandle(bool distinct, Expression body, params Type[] arr)
        {
            _sqlCaluse.Clear();
            _sqlCaluse.EnableTableAlia = true;
            foreach (Type item in arr)
            {
                _sqlCaluse.SetTableAlias(item);
            }
            var mainTable = typeof(T);
            var tableName = _sqlCaluse.GetTableName(mainTable) + " " + _sqlCaluse.GetTableAlias(mainTable).Replace(".", "");
            ExpressionVisit.Select(body, _sqlCaluse);
            _sqlCaluse += " SELECT ";
            if (distinct) _sqlCaluse += "DISTINCT ";
            selectContentIndexStart = _sqlCaluse.Length;
            _sqlCaluse += _sqlCaluse.SelectedFieldString;
            selectContentIndexEnd = _sqlCaluse.Length;
            _sqlCaluse += "\n FROM ";
            _sqlCaluse += tableName;
        }

        private void JoinHandle<T1>(string joinType, Expression exp)
        {
            var joinTable = typeof(T1);
            _sqlCaluse.SetTableAlias(joinTable);
            var tableName = _sqlCaluse.GetTableName(joinTable);
            _sqlCaluse += $"{joinType} JOIN {tableName} {_sqlCaluse.GetTableAlias(joinTable).Replace(".", "")} ON (";
            ExpressionVisit.Join(exp, _sqlCaluse);
            _sqlCaluse += ")";
        }

        private void WhereHandle(Expression body)
        {
            if (_firstWhere)
            {
                _sqlCaluse += "\n WHERE";
                _firstWhere = false;
            }
            else
            {
                _sqlCaluse += "\n AND";
            }
            _sqlCaluse += "(";
            if (body?.ToString() == "True")
            {
                _sqlCaluse += " 1 = 1 ";
            }
            else
            {
                ExpressionVisit.Where(body, _sqlCaluse);
            }
            _sqlCaluse += ")";
        }

        private void UpdateHandle(Expression body, Expression pkExp = null)
        {
            _sqlCaluse.EnableTableAlia = false;
            var tableName = _sqlCaluse.GetTableName(typeof(T));
            _sqlCaluse += $" UPDATE {tableName} SET \n";
            if (null != pkExp)
                ExpressionVisit.PrimaryKey(pkExp, _sqlCaluse);
            ExpressionVisit.Update(body, _sqlCaluse);
        }

        private string InsertHandle()
        {
            _sqlCaluse.EnableTableAlia = false;
            var tableName = _sqlCaluse.GetTableName(typeof(T));
            return $" INSERT INTO {tableName} ({{0}}) \n VALUES ({{1}})";
        }

        private string DeleteHandle()
        {
            _sqlCaluse.EnableTableAlia = false;
            var tableName = _sqlCaluse.GetTableName(typeof(T));
            return $" DELETE FROM {tableName} \n";
        }

        private void OrderByHandle(Expression exp)
        {
            if (_firstOrderby)
            {
                _sqlCaluse += "\n ORDER BY ";
                _firstOrderby = false;
            }
            else
                _sqlCaluse += " ,";
            ExpressionVisit.OrderBy(exp, _sqlCaluse);
        }
        private void GroupByHandle(Expression body, bool rollup)
        {
            ExpressionVisit.GroupBy(body, _sqlCaluse);
            if (rollup)
            {
                _sqlCaluse += $"\n GROUP BY ROLLUP ({_sqlCaluse.GroupByFieldString})";
            }
            else
            {
                _sqlCaluse += "\n GROUP BY " + _sqlCaluse.GroupByFieldString;
            }
        }

        private void CountHandle()
        {
            var tbType = typeof(T);
            _sqlCaluse.SetTableAlias(tbType);
            var tbName = _sqlCaluse.GetTableName(tbType);
            var alia = _sqlCaluse.GetTableAlias(tbType).Replace(".", "");
            _sqlCaluse += $" SELECT COUNT(*) FROM {tbName} {alia}";
        }

        private void MaxHandle(Expression body)
        {
            var tbType = typeof(T);
            var tbName = _sqlCaluse.GetTableName(tbType);
            _sqlCaluse.SetTableAlias(tbType);
            var alia = _sqlCaluse.GetTableAlias(tbType).Replace(".", "");
            ExpressionVisit.Max(body, _sqlCaluse);
            var sql = $" SELECT {{0}} FROM {tbName} {alia}";
            _sqlCaluse.Sql.AppendFormat(sql, _sqlCaluse.SelectMethod.ToString());
        }
    }
}

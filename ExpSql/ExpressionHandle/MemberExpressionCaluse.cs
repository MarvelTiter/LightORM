using MDbEntity.Attributes;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace DExpSql.ExpressionHandle {
    class MemberExpressionCaluse : BaseExpressionSql<MemberExpression> {
        protected override SqlCaluse Where(MemberExpression exp, SqlCaluse sqlCaluse) {
            sqlCaluse += FindValue(exp, sqlCaluse);
            return sqlCaluse;
        }

        protected override SqlCaluse Select(MemberExpression exp, SqlCaluse sqlCaluse) {
            string col = CustomHandle(exp, sqlCaluse);
            sqlCaluse.SelectFields.Add(col);
            return sqlCaluse;
        }

        protected override SqlCaluse Max(MemberExpression exp, SqlCaluse sqlCaluse) {
            var col = CustomHandle(exp, sqlCaluse);
            sqlCaluse.SelectMethod.Append($"MAX({col})");
            return sqlCaluse;
        }

        protected override SqlCaluse SelectMethod(MemberExpression exp, SqlCaluse sqlCaluse) {
            var temp = FindValue(exp, sqlCaluse);
            sqlCaluse.SelectMethod.Append(temp);
            return sqlCaluse;
        }

        protected override SqlCaluse Join(MemberExpression exp, SqlCaluse sqlCaluse) {
            string col = CustomHandle(exp, sqlCaluse);
            sqlCaluse += col;
            return sqlCaluse;
        }

        protected override SqlCaluse OrderBy(MemberExpression exp, SqlCaluse sqlCaluse) {
            string col = CustomHandle(exp, sqlCaluse);
            sqlCaluse += col;
            sqlCaluse.HasOrderBy = true;
            return sqlCaluse;
        }

        protected override SqlCaluse GroupBy(MemberExpression exp, SqlCaluse sqlCaluse) {
            string col = CustomHandle(exp, sqlCaluse);
            if (!sqlCaluse.GroupByFields.Contains(col))
                sqlCaluse.GroupByFields.Add(col);
            return sqlCaluse;
        }

        protected override SqlCaluse PrimaryKey(MemberExpression exp, SqlCaluse sqlCaluse) {
            var name = exp.Member.Name;
            if (!sqlCaluse.IgnoreFields.Contains(name))
                sqlCaluse.IgnoreFields.Add(name);
            return sqlCaluse;
        }

        protected override SqlCaluse Update(MemberExpression exp, SqlCaluse sqlCaluse) {

            var e = Expression.Lambda(exp).Compile().DynamicInvoke();
            var eType = e.GetType();
            var props = eType.GetProperties();
            for (int i = 0; i < props.Length; i++) {
                var p = props[i];
                var value = p.GetValue(e, null);
                if (value == null || value == default || value == DBNull.Value)
                    continue;
                if (sqlCaluse.IgnoreFields.Contains(p.Name))
                    continue;
                if (p.GetCustomAttributes(typeof(PrimaryKeyAttribute), false).Length > 0)
                    continue;
                sqlCaluse += $" {p.Name} = ";
                sqlCaluse += sqlCaluse.AddDbParameter(value);
                sqlCaluse += ",\n";
            }
            if (sqlCaluse.EndWith(",\n")) {
                sqlCaluse.Sql.Remove(sqlCaluse.Sql.Length - 2, 2);
            }
            return sqlCaluse;
        }

        protected override SqlCaluse Insert(MemberExpression exp, SqlCaluse sqlCaluse) {
            var e = Expression.Lambda(exp).Compile().DynamicInvoke();
            var eType = exp.Type;
            var props = eType.GetProperties();
            for (int i = 0; i < props.Length; i++) {
                var p = props[i];
                var value = p.GetValue(e, null);
                if (value == null || value == DBNull.Value)
                    continue;
                sqlCaluse.SelectFields.Add(p.Name);
                sqlCaluse.AddDbParameter(value);
            }
            return sqlCaluse;
        }

        protected override SqlCaluse In(MemberExpression exp, SqlCaluse sqlCaluse) {
            var v = Expression.Lambda(exp).Compile().DynamicInvoke();
            IEnumerable array = v as IEnumerable;
            foreach (var item in array) {
                sqlCaluse += $"'{item}', ";
            }
            sqlCaluse -= ", ";
            return sqlCaluse;
        }

        private string CustomHandle(MemberExpression exp, SqlCaluse sqlCaluse) {
            var table = exp.Member.DeclaringType;
            sqlCaluse.SetTableAlias(table);
            var alias = sqlCaluse.GetTableAlias(table);
            return $"{alias}.{exp.Member.Name}";
        }

        private string FindValue(MemberExpression exp, SqlCaluse sqlCaluse) {
            if (exp.Expression.NodeType == ExpressionType.Parameter) {
                string tableAlias = sqlCaluse.GetTableAlias(exp.Member.DeclaringType);
                if (!string.IsNullOrWhiteSpace(tableAlias)) {
                    tableAlias += ".";
                }
                return " " + tableAlias + exp.Member.Name;
            } else {
                //var v = exp.Expression as ConstantExpression;
                var v = Expression.Lambda(exp).Compile().DynamicInvoke();
                return sqlCaluse.AddDbParameter(v);
            }
        }
    }
}

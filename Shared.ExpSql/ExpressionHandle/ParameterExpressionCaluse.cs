using DExpSql;
using MDbEntity.Attributes;
using Shared.ExpSql.Extension;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ExpSql.ExpressionHandle {
    class ParameterExpressionCaluse : BaseExpressionSql<ParameterExpression> {
        protected override SqlCaluse Select(ParameterExpression exp, SqlCaluse sqlCaluse) {
            var t = exp.Type;
            var props = t.GetProperties();
            sqlCaluse.SetTableAlias(t);
            var alia = sqlCaluse.GetTableAlias(t);
            foreach (PropertyInfo item in props) {
                var col = item.GetAttribute<ColumnNameAttribute>();
                if (col == null)
                    sqlCaluse.SelectFields.Add($"{alia}.{item.Name}");
                else
                    sqlCaluse.SelectFields.Add($"{alia}.{col.Name} {item.Name}");

            }
            return sqlCaluse;
        }
    }
}

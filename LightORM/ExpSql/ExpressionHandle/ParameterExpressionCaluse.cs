using DExpSql;
using MDbContext.Extension;
using MDbEntity.Attributes;
using System.Linq.Expressions;
using System.Reflection;

namespace ExpSql.ExpressionHandle {
    class ParameterExpressionCaluse : BaseExpressionSql<ParameterExpression> {
        protected override SqlCaluse Select(ParameterExpression exp, SqlCaluse sqlCaluse) {
            var t = exp.Type;
            var props = t.GetProperties();
            sqlCaluse.SetTableAlias(t);
            var alia = sqlCaluse.GetTableAlias(t);
            foreach (PropertyInfo item in props) {
                if (item.GetAttribute<IgnoreAttribute>() != null)
                    continue;
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

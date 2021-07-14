using MDbContext.Extension;
using MDbContext.NewExpSql.SqlFragment;
using MDbEntity.Attributes;
using System.Linq.Expressions;
using System.Reflection;

namespace MDbContext.NewExpSql.ExpressionParser {
    internal class ParameterExpressionParser : BaseParser<ParameterExpression> {
        protected override BaseFragment Select(ParameterExpression exp, ISqlContext context, SelectFragment fragment) {
            var t = exp.Type;
            var props = t.GetProperties();
            context.SetTableAlias(t);
            var alia = context.GetTableAlias(t);
            foreach (PropertyInfo item in props) {
                if (item.GetAttribute<IgnoreAttribute>() != null)
                    continue;
                var col = item.GetAttribute<ColumnNameAttribute>();
                if (col == null)
                    fragment.SelectedFields.Add($"{alia}.{item.Name}");
                else
                    fragment.SelectedFields.Add($"{alia}.{col.Name} {item.Name}");

            }
            return fragment;
        }
    }
}

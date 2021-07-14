using MDbContext.Extension;
using MDbContext.NewExpSql.SqlFragment;
using MDbEntity.Attributes;
using System.Linq.Expressions;

namespace MDbContext.NewExpSql.ExpressionParser {
    internal class MemberExpressionParser : BaseParser<MemberExpression> {
        protected override BaseFragment Select(MemberExpression exp, ISqlContext context, SelectFragment sqlFragment) {
            if (exp.Member.GetAttribute<IgnoreAttribute>() == null) {
                string col = GetMemberName(exp, context);
                sqlFragment.SelectedFields.Add(col);
            }
            return sqlFragment;
        }

        public override BaseFragment Where(MemberExpression exp, ISqlContext context, WhereFragment fragment) {
            if (context.Position == Position.Left) {
                var col = GetMemberName(exp, context);
                fragment.Add(col);
            } else if (context.Position == Position.Right) {
                var p = GetParameterName(exp, context);
                fragment.Add(p);
            }
            return fragment;
        }

        public override BaseFragment Join(MemberExpression exp, ISqlContext context, JoinFragment fragment) {
            var col = GetMemberName(exp, context);
            fragment.Add(col);
            return fragment;

        }

        private string GetMemberName(MemberExpression exp, ISqlContext context, bool aliaReqired = true) {
            var table = exp.Member.DeclaringType;
            context.SetTableAlias(table);
            var alias = context.GetTableAlias(table);
            var attr = exp.Member.GetAttribute<ColumnNameAttribute>();
            if (attr != null) {
                if (aliaReqired)
                    return $"{alias}.{attr.Name} {exp.Member.Name}";
                else
                    return $"{alias}.{attr.Name}";
            } else
                return $"{alias}.{exp.Member.Name}";
        }
        private string GetParameterName(MemberExpression exp, ISqlContext context) {
            var v = Expression.Lambda(exp).Compile().DynamicInvoke();
            return context.AddDbParameter(v);
        }
    }
}

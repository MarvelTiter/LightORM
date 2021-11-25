using MDbContext.Extension;
using MDbContext.NewExpSql.SqlFragment;
using MDbEntity.Attributes;
using System.Linq.Expressions;

namespace MDbContext.NewExpSql.ExpressionParser
{
    internal class MemberExpressionParser : BaseParser<MemberExpression>
    {
        protected override BaseFragment Select(MemberExpression exp, SelectFragment fragment)
        {
            if (exp.Member.GetAttribute<IgnoreAttribute>() == null)
            {
                string col = GetMemberName(exp, fragment.Tables);
                fragment.SelectedFields.Add(col);
            }
            return fragment;
        }

        public override BaseFragment Where(MemberExpression exp, WhereFragment fragment)
        {
            if (fragment.Position == Position.Left)
            {
                var col = GetMemberName(exp, fragment.Tables);
                fragment.SqlAppend(col);
                return fragment;
            }
            else if (fragment.Position == Position.Right)
            {
                var p = GetParameterName(exp, fragment.Tables);
                fragment.SqlAppend(p);
            }
            return fragment;
        }

        public override BaseFragment Join(MemberExpression exp, JoinFragment fragment)
        {
            var col = GetMemberName(exp, fragment.Tables, false);
            fragment.SqlAppend(col);
            return fragment;
        }

        private static string GetMemberName(MemberExpression exp, ITableContext context, bool aliaReqired = true)
        {
            var table = exp.Member.DeclaringType;
            context.SetTableAlias(table);
            var alias = context.GetTableAlias(table);
            var attr = exp.Member.GetAttribute<ColumnNameAttribute>();
            if (attr != null)
            {
                if (aliaReqired)
                    return $"{alias}.{attr.Name} {exp.Member.Name}";
                else
                    return $"{alias}.{attr.Name}";
            }
            else
                return $"{alias}.{exp.Member.Name}";
        }

        private static string GetParameterName(MemberExpression exp, ITableContext context)
        {
            return $"{context.GetPrefix()}{exp.Member.Name}";
        }
    }
}

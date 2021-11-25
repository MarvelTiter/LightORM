using System;
using System.Linq.Expressions;
using System.Text;

namespace MDbContext.NewExpSql.SqlFragment
{
    internal enum JoinType
    {
        Inner,
        Left,
        Right
    }
    internal class JoinFragment : BaseFragment
    {
        private readonly JoinType joinType;
        private readonly Type type;

        public JoinFragment(JoinType joinType, Type type)
        {
            this.joinType = joinType;
            this.type = type;
        }

        protected override void DoResolve(Expression body, params Type[] types)
        {
            Tables.SetTableAlias(type);
            Sql.Append($"{joinType.ToString().ToUpper()} JOIN {Tables.GetTableName(true, type)}");
            ExpressionVisit.Join(body, this);
        }
    }
}

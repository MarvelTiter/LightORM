using System;
using System.Linq.Expressions;
using System.Text;

namespace MDbContext.NewExpSql.SqlFragment {
    internal enum JoinType {
        Inner,
        Left,
        Right
    }
    internal class JoinFragment : BaseFragment {
        private readonly ISqlContext context;
        private readonly JoinType joinType;
        private readonly Type type;

        public JoinFragment(ISqlContext context, JoinType joinType, Type type) {
            this.context = context;
            this.joinType = joinType;
            this.type = type;
        }

        protected override void DoResolve(Expression body, params Type[] types) {
            Sql.Append($"{joinType.ToString().ToUpper()} JOIN {context.GetTableName(true, type)}");
            ExpressionVisit.Join(body, context, this);
        }

        public override string ToString() {
            return Sql.ToString();
        }
    }
}

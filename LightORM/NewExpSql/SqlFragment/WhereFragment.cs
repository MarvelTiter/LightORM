using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace MDbContext.NewExpSql.SqlFragment {
    internal class WhereFragment : BaseFragment {
        private readonly ISqlContext context;

        public WhereFragment(ISqlContext context) {
            this.context = context;
        }
        internal override void ResolveSql(Expression body, params Type[] types) {
            Sql.Clear();
            DoResolve(body, types);
        }
        protected override void DoResolve(Expression body, params Type[] types) {
            if (context.WhereIndex == 0) {
                Add("Where");
            } else {
                Add("AND");
            }
            Add(" ( ");
            ExpressionVisit.Where(body,context, this);
            Add(" ) ");
        }

        public override string ToString() {
            return Sql.ToString();
        }
    }
}

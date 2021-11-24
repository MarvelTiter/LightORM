using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace MDbContext.NewExpSql.SqlFragment
{
    internal class WhereFragment : BaseFragment
    {

        internal override void ResolveSql(Expression body, params Type[] types)
        {
            Sql.Clear();
            DoResolve(body, types);
        }
        protected override void DoResolve(Expression body, params Type[] types)
        {
            SqlAppend("Where");
            SqlAppend(" ( ");
            ExpressionVisit.Where(body, this);
            SqlAppend(" ) ");
        }

        internal override void ResolveParam()
        {

        }
    }
}

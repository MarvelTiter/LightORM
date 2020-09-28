using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace DExpSql
{
    internal interface IExpressionSql
    {
        SqlCaluse Update(Expression expression, SqlCaluse SqlCaluse);

        SqlCaluse PrimaryKey(Expression expression, SqlCaluse SqlCaluse);

        SqlCaluse Select(Expression expression, SqlCaluse SqlCaluse);

        SqlCaluse Join(Expression expression, SqlCaluse SqlCaluse);

        SqlCaluse Where(Expression expression, SqlCaluse SqlCaluse);

        SqlCaluse Insert(Expression expression, SqlCaluse SqlCaluse);

        SqlCaluse In(Expression expression, SqlCaluse SqlCaluse);

        SqlCaluse GroupBy(Expression expression, SqlCaluse SqlCaluse);

        SqlCaluse OrderBy(Expression expression, SqlCaluse SqlCaluse);

    }
}

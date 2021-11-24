using MDbContext.NewExpSql.SqlFragment;
using System.Linq.Expressions;

namespace MDbContext.NewExpSql
{
    internal interface IExpressionParser
    {
        BaseFragment Select(Expression expression, BaseFragment SqlFragment);
        BaseFragment Insert(Expression expression, BaseFragment SqlFragment);
        BaseFragment Update(Expression expression, BaseFragment SqlFragment);
        BaseFragment Join(Expression expression, BaseFragment SqlFragment);
        BaseFragment Where(Expression expression, BaseFragment SqlFragment);
        BaseFragment GroupBy(Expression expression, BaseFragment SqlFragment);
        BaseFragment OrderBy(Expression expression, BaseFragment SqlFragment);
    }
}

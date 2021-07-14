using MDbContext.NewExpSql.SqlFragment;
using System.Linq.Expressions;

namespace MDbContext.NewExpSql {
    internal interface IExpressionParser {
        BaseFragment Select(Expression expression, ISqlContext context, BaseFragment SqlFragment);
        BaseFragment Insert(Expression expression, ISqlContext context, BaseFragment SqlFragment);
        BaseFragment Update(Expression expression, ISqlContext context, BaseFragment SqlFragment);
        BaseFragment Join(Expression expression, ISqlContext context, BaseFragment SqlFragment);
        BaseFragment Where(Expression expression, ISqlContext context, BaseFragment SqlFragment);
        BaseFragment GroupBy(Expression expression, ISqlContext context, BaseFragment SqlFragment);
        BaseFragment OrderBy(Expression expression, ISqlContext context, BaseFragment SqlFragment);
    }
}

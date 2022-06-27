using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace MDbContext.ExpressionSql.ExpressionVisitor;

internal abstract class BaseVisitor<T> : IExpressionVisitor where T : Expression
{
    private string _expressionType => typeof(T).Name;
    public void Visit(Expression expression, SqlConfig config, SqlContext context)
    {
        DoVisit((T)expression, config, context);
    }

    public abstract void DoVisit(T exp, SqlConfig config, SqlContext context);
    #region select
    public void Select(Expression exp, SqlContext context) => Select((T)exp, context);
    public virtual void Select(T exp, SqlContext context) =>
        throw new NotImplementedException($"[{_expressionType}] 未实现 {nameof(Select)}");

    public void DbFunc(Expression exp, SqlContext context) => DbFunc((T)exp, context);
    public virtual void DbFunc(T exp, SqlContext context) =>
        throw new NotImplementedException($"[{_expressionType}] 未实现 {nameof(DbFunc)}");

    #endregion

    #region update
    public void Update(Expression exp, SqlContext context) => Update((T)exp, context);
    public virtual void Update(T exp, SqlContext context) =>
       throw new NotImplementedException($"[{_expressionType}] 未实现 {nameof(Update)}");
    public void UpdateIgnore(Expression exp, SqlContext context) => UpdateIgnore((T)exp, context);
    public virtual void UpdateIgnore(T exp, SqlContext context) =>
       throw new NotImplementedException($"[{_expressionType}] 未实现 {nameof(UpdateIgnore)}");

    #endregion
}

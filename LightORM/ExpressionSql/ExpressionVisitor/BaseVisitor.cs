using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace LightORM.ExpressionSql.ExpressionVisitor;

internal abstract class BaseVisitor<T> : IExpressionVisitor where T : Expression
{
    private string _expressionType => typeof(T).Name;
    public void Visit(Expression expression, SqlResolveOptions config, SqlContext context)
    {
        DoVisit((T)expression, config, context);
    }

    public abstract void DoVisit(T exp, SqlResolveOptions config, SqlContext context);
    public void Select(Expression exp, SqlContext context) => Select((T)exp, context);
    public virtual void Select(T exp, SqlContext context) =>
        throw new NotImplementedException($"[{_expressionType}] 未实现 {nameof(Select)}");

    public void DbFunc(Expression exp, SqlContext context) => DbFunc((T)exp, context);
    public virtual void DbFunc(T exp, SqlContext context) =>
        throw new NotImplementedException($"[{_expressionType}] 未实现 {nameof(DbFunc)}");


    public void Update(Expression exp, SqlContext context) => Update((T)exp, context);
    public virtual void Update(T exp, SqlContext context) =>
       throw new NotImplementedException($"[{_expressionType}] 未实现 {nameof(Update)}");
    public void UpdateIgnore(Expression exp, SqlContext context) => UpdateIgnore((T)exp, context);
    public virtual void UpdateIgnore(T exp, SqlContext context) =>
       throw new NotImplementedException($"[{_expressionType}] 未实现 {nameof(UpdateIgnore)}");


    public void Join(Expression exp, SqlContext context) => Join((T)exp, context);
    public virtual void Join(T exp, SqlContext context) =>
        throw new NotImplementedException($"[{_expressionType}] 未实现 {nameof(Join)}");

    public void Insert(Expression exp, SqlContext context) => Insert((T)exp, context);
    public virtual void Insert(T exp, SqlContext context) =>
        throw new NotImplementedException($"[{_expressionType}] 未实现 {nameof(Insert)}");

    public void Where(Expression exp, SqlContext context) => Where((T)exp, context);
    public virtual void Where(T exp, SqlContext context) =>
        throw new NotImplementedException($"[{_expressionType}] 未实现 {nameof(Where)}");

}

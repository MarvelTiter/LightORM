using System.Collections.Generic;
using System.Linq.Expressions;

namespace LightORM.ExpressionSql;

internal interface IExpressionVisitor
{
    void Visit(Expression expression, SqlConfig config, SqlContext context);
    //void Select(Expression exp, SqlContext context);
    //void DbFunc(Expression exp, SqlContext context);
    //void Update(Expression exp, SqlContext context);
    //void UpdateIgnore(Expression exp, SqlContext context);
    //void Join(Expression exp, SqlContext context);
    //void Insert(Expression exp, SqlContext context);
    //void Where(Expression exp, SqlContext context);

}

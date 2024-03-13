using LightORM.ExpressionSql;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace MDbContext.Repository;

public partial interface IRepository<T>
{
    //IRepository<T> SwitchDatabase(string key);
    T? Insert(T item);
    int Update(T item, Expression<Func<T, bool>>? whereExpression);
    int Update(Expression<Func<object>> updateExpression, Expression<Func<T, bool>>? whereExpression);
    int Delete(Expression<Func<T, bool>>? whereExpression);
    int Count(Expression<Func<T, bool>>? whereExpression);
    TMember Max<TMember>(Expression<Func<T, TMember>> maxExpression, Expression<Func<T, bool>>? whereExpression);
    T? GetSingle(Expression<Func<T, bool>>? whereExpression);
    IEnumerable<T> GetList(Expression<Func<T, bool>>? whereExpression, out long total, int index = 0, int size = 0, Expression<Func<T, object>>? orderByExpression = null, bool asc = true);
    IEnumerable<T> GetList(Expression<Func<T, bool>>? whereExpression, Expression<Func<T, object>>? orderByExpression = null, bool asc = true);
}

using LightORM.ExpressionSql.Interface.Select;

namespace LightORM.Providers.Select;

internal class SelectProvider1<T1> : SelectProvider0<IExpSelect<T1>, T1>, IExpSelect<T1>
{
    public SelectProvider1(Expression exp, ISqlExecutor executor) : base(executor)
    {
        SelectExpression = new ExpressionInfo()
        {
            Expression = exp,
            ResolveOptions = SqlResolveOptions.Select
        };
        SqlBuilder.Expressions.Add(SelectExpression);
    }
    public IExpSelect<T1> GroupBy(Expression<Func<T1, object>> exp)
    {
        return GroupByHandle(exp);
    }

    public IExpSelect<T1> GroupByIf(bool ifGroupby, Expression<Func<T1, bool>> exp)
    {
        if (ifGroupby)
        {
            return GroupByHandle(exp);
        }
        return this;
    }

    public IExpSelect<T1> OrderBy(Expression<Func<T1, object>> exp, bool asc = true)
    {
        return OrderByHandle(exp, asc);
    }
}

internal class SelectProvider2<T1, T2> : SelectProvider0<IExpSelect<T1, T2>, T1>, IExpSelect<T1, T2>
{
    public SelectProvider2(Expression exp, ISqlExecutor executor) : base(executor)
    {
    }

    public IExpSelect<T1, T2> GroupBy(Expression<Func<T1, T2, object>> exp)
    {
        throw new NotImplementedException();
    }

    public IExpSelect<T1, T2> InnerJoin<TAnother>(Expression<Func<ExpressionSql.Interface.Select.TypeSet<TAnother, T1, T2>, bool>> exp)
    {
        throw new NotImplementedException();
    }

    public IExpSelect<T1, T2> LeftJoin<TAnother>(Expression<Func<ExpressionSql.Interface.Select.TypeSet<TAnother, T1, T2>, bool>> exp)
    {
        throw new NotImplementedException();
    }

    public IExpSelect<T1, T2> OrderBy(Expression<Func<T1, T2, object>> exp, bool asc = true)
    {
        throw new NotImplementedException();
    }

    public IExpSelect<T1, T2> RightJoin<TAnother>(Expression<Func<ExpressionSql.Interface.Select.TypeSet<TAnother, T1, T2>, bool>> exp)
    {
        throw new NotImplementedException();
    }

    public IExpSelect<T1, T2> Where(Expression<Func<T1, T2, bool>> exp)
    {
        throw new NotImplementedException();
    }
}

internal class SelectProvider3<T1, T2, T3> : SelectProvider0<IExpSelect<T1, T2, T3>, T1>, IExpSelect<T1, T2, T3>
{
    public SelectProvider3(Expression exp, ISqlExecutor executor) : base(executor)
    {
    }

    public IExpSelect<T1, T2, T3> GroupBy(Expression<Func<TypeSet<T1, T2, T3>, object>> exp)
    {
        throw new NotImplementedException();
    }

    public IExpSelect<T1, T2, T3> InnerJoin<TJoin>(Expression<Func<TypeSet<T1, T2, T3>, bool>> exp)
    {
        throw new NotImplementedException();
    }

    public IExpSelect<T1, T2, T3> LeftJoin<TJoin>(Expression<Func<TypeSet<T1, T2, T3>, bool>> exp)
    {
        throw new NotImplementedException();
    }

    public IExpSelect<T1, T2, T3> OrderBy(Expression<Func<TypeSet<T1, T2, T3>, object>> exp, bool asc = true)
    {
        throw new NotImplementedException();
    }

    public IExpSelect<T1, T2, T3> RightJoin<TJoin>(Expression<Func<TypeSet<T1, T2, T3>, bool>> exp)
    {
        throw new NotImplementedException();
    }

    public IExpSelect<T1, T2, T3> Where(Expression<Func<TypeSet<T1, T2, T3>, bool>> exp)
    {
        throw new NotImplementedException();
    }
}

internal class SelectProvider4<T1, T2, T3, T4> : SelectProvider0<IExpSelect<T1, T2, T3, T4>, T1>, IExpSelect<T1, T2, T3, T4>
{
    public SelectProvider4(ISqlExecutor executor) : base(executor)
    {
    }

    public IExpSelect<T1, T2, T3, T4> GroupBy(Expression<Func<TypeSet<T1, T2, T3, T4>, object>> exp)
    {
        throw new NotImplementedException();
    }

    public IExpSelect<T1, T2, T3, T4> InnerJoin<TJoin>(Expression<Func<TypeSet<T1, T2, T3, T4>, bool>> exp)
    {
        throw new NotImplementedException();
    }

    public IExpSelect<T1, T2, T3, T4> LeftJoin<TJoin>(Expression<Func<TypeSet<T1, T2, T3, T4>, bool>> exp)
    {
        throw new NotImplementedException();
    }

    public IExpSelect<T1, T2, T3, T4> OrderBy(Expression<Func<TypeSet<T1, T2, T3, T4>, object>> exp, bool asc = true)
    {
        throw new NotImplementedException();
    }

    public IExpSelect<T1, T2, T3, T4> RightJoin<TJoin>(Expression<Func<TypeSet<T1, T2, T3, T4>, bool>> exp)
    {
        throw new NotImplementedException();
    }

    public IExpSelect<T1, T2, T3, T4> Where(Expression<Func<TypeSet<T1, T2, T3, T4>, bool>> exp)
    {
        throw new NotImplementedException();
    }
}
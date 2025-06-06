using System.Threading;

namespace LightORM.Providers.Select;

internal sealed class SelectProvider2<T1, T2> : SelectProvider0<IExpSelect<T1, T2>, T1>, IExpSelect<T1, T2>
{
    public SelectProvider2(ISqlExecutor executor, SelectBuilder? builder = null)
        : base(executor, builder)
    {
        if (builder == null)
        {
            SqlBuilder = new SelectBuilder(DbType);
            SqlBuilder.SelectedTables.Add(TableInfo.Create<T1>(0));
            SqlBuilder.SelectedTables.Add(TableInfo.Create<T2>(1));
        }
    }

    public IExpSelectGroup<TGroup, TypeSet<T1, T2>> GroupBy<TGroup>(Expression<Func<T1, T2, TGroup>> exp)
    {
        return this.GroupByHandle<TGroup, TypeSet<T1, T2>>(exp);
    }

    public IExpSelect<T1, T2> OrderBy(Expression<Func<T1, T2, object>> exp)
    {
        this.OrderByHandle(exp, true);
        return this;
    }

    public IExpSelect<T1, T2> OrderByDesc(Expression<Func<T1, T2, object>> exp)
    {
        this.OrderByHandle(exp, false);
        return this;
    }
    public IExpSelect<T1, T2> Where(Expression<Func<T1, T2, bool>> exp)
    {
        this.WhereHandle(exp);
        return this;
    }
    
    #region join
    public IExpSelect<T1, T2, TJoin> InnerJoin<TJoin>(Expression<Func<T1, T2, TJoin, bool>> exp)
    {
        this.JoinHandle<TJoin>(exp, ExpressionSql.TableLinkType.InnerJoin);
        return new SelectProvider3<T1, T2, TJoin>(Executor, SqlBuilder);
    }

    public IExpSelect<T1, T2, TJoin> LeftJoin<TJoin>(Expression<Func<T1, T2, TJoin, bool>> exp)
    {
        this.JoinHandle<TJoin>(exp, ExpressionSql.TableLinkType.LeftJoin);
        return new SelectProvider3<T1, T2, TJoin>(Executor, SqlBuilder);
    }
    public IExpSelect<T1, T2, TJoin> RightJoin<TJoin>(Expression<Func<T1, T2, TJoin, bool>> exp)
    {
        this.JoinHandle<TJoin>(exp, ExpressionSql.TableLinkType.RightJoin);
        return new SelectProvider3<T1, T2, TJoin>(Executor, SqlBuilder);
    }
    public IExpSelect<T1, T2, TJoin> InnerJoin<TJoin>(IExpSelect<TJoin> subQuery, Expression<Func<T1, T2, TJoin, bool>> where)
    {
        this.JoinHandle<TJoin>(where, ExpressionSql.TableLinkType.InnerJoin, subQuery);
        return new SelectProvider3<T1, T2, TJoin>(Executor, SqlBuilder);
    }

    public IExpSelect<T1, T2, TJoin> LeftJoin<TJoin>(IExpSelect<TJoin> subQuery, Expression<Func<T1, T2, TJoin, bool>> where)
    {
        this.JoinHandle<TJoin>(where, ExpressionSql.TableLinkType.LeftJoin, subQuery);
        return new SelectProvider3<T1, T2, TJoin>(Executor, SqlBuilder);
    }
    public IExpSelect<T1, T2, TJoin> RightJoin<TJoin>(IExpSelect<TJoin> subQuery, Expression<Func<T1, T2, TJoin, bool>> where)
    {
        this.JoinHandle<TJoin>(where, ExpressionSql.TableLinkType.RightJoin, subQuery);
        return new SelectProvider3<T1, T2, TJoin>(Executor, SqlBuilder);
    }

    #endregion

    #region result

    public IEnumerable<TReturn> ToList<TReturn>(Expression<Func<T1, T2, TReturn>> exp)
    {
        this.HandleResult(exp, null);
        return this.InternalToList<TReturn>();
    }
    public Task<IList<TReturn>> ToListAsync<TReturn>(Expression<Func<T1, T2, TReturn>> exp, CancellationToken cancellationToken = default)
    {
        this.HandleResult(exp, null);
        return this.InternalToListAsync<TReturn>(cancellationToken);
    }

    public IEnumerable<TReturn> ToList<TReturn>(Expression<Func<T1, T2, object>> exp)
    {
        this.HandleResult(exp, null);
        return this.InternalToList<TReturn>();
    }
    public Task<IList<TReturn>> ToListAsync<TReturn>(Expression<Func<T1, T2, object>> exp, CancellationToken cancellationToken = default)
    {
        this.HandleResult(exp, null);
        return this.InternalToListAsync<TReturn>(cancellationToken);
    }

    public IAsyncEnumerable<TReturn> ToEnumerableAsync<TReturn>(Expression<Func<T1, T2, TReturn>> exp, CancellationToken cancellationToken = default)
    {
        this.HandleResult(exp, null);
        return this.InternalToEnumerableAsync<TReturn>(cancellationToken);
    }

    public IAsyncEnumerable<TReturn> ToEnumerableAsync<TReturn>(Expression<Func<T1, T2, object>> exp, CancellationToken cancellationToken = default)
    {
        this.HandleResult(exp, null);
        return this.InternalToEnumerableAsync<TReturn>(cancellationToken);
    }

    #endregion

    //public IExpSelect<TTemp> AsSubQuery<TTemp>(Expression<Func<T1, T2, TTemp>> exp, string? alias = null)
    //{
    //    this.HandleResult(exp, null);
    //    return this.HandleSubQuery<TTemp>(alias);
    //}

    public IExpSelect<TTable> AsTable<TTable>(Expression<Func<T1, T2, TTable>> exp)
    {
        this.HandleResult(exp, null);
        return new SelectProvider1<TTable>(Executor, SqlBuilder);
    }

    public IExpTemp<TTemp> AsTemp<TTemp>(string name, Expression<Func<T1, T2, TTemp>> exp)
    {
        this.HandleResult(exp, null);
        return new TempProvider<TTemp>(name, SqlBuilder);
    }

    public string ToSql(Expression<Func<T1, T2, object>> exp)
    {
        this.HandleResult(exp, null);
        return ToSql();
    }

    #region with temp

    public IExpSelect<T1, T2, TTemp> WithTempQuery<TTemp>(IExpTemp<TTemp> temp)
    {
        this.HandleTempQuery(temp);
        return new SelectProvider3<T1, T2, TTemp>(Executor, SqlBuilder);
    }

    public IExpSelect<T1, T2, TTemp1, TTemp2> WithTempQuery<TTemp1, TTemp2>(IExpTemp<TTemp1> temp1, IExpTemp<TTemp2> temp2)
    {
        this.HandleTempQuery(temp1, temp2);
        return new SelectProvider4<T1, T2, TTemp1, TTemp2>(Executor, SqlBuilder);
    }

    public IExpSelect<T1, T2, TTemp1, TTemp2, TTemp3> WithTempQuery<TTemp1, TTemp2, TTemp3>(IExpTemp<TTemp1> temp1, IExpTemp<TTemp2> temp2, IExpTemp<TTemp3> temp3)
    {
        this.HandleTempQuery(temp1, temp2, temp3);
        return new SelectProvider5<T1, T2, TTemp1, TTemp2, TTemp3>(Executor, SqlBuilder);
    }

    public IExpSelect<T1, T2, TTemp1, TTemp2, TTemp3, TTemp4> WithTempQuery<TTemp1, TTemp2, TTemp3, TTemp4>(IExpTemp<TTemp1> temp1, IExpTemp<TTemp2> temp2, IExpTemp<TTemp3> temp3, IExpTemp<TTemp4> temp4)
    {
        this.HandleTempQuery(temp1, temp2, temp3, temp4);
        return new SelectProvider6<T1, T2, TTemp1, TTemp2, TTemp3, TTemp4>(Executor, SqlBuilder);
    }

    public IExpSelect<T1, T2, TTemp1, TTemp2, TTemp3, TTemp4, TTemp5> WithTempQuery<TTemp1, TTemp2, TTemp3, TTemp4, TTemp5>(IExpTemp<TTemp1> temp1, IExpTemp<TTemp2> temp2, IExpTemp<TTemp3> temp3, IExpTemp<TTemp4> temp4, IExpTemp<TTemp5> temp5)
    {
        this.HandleTempQuery(temp1, temp2, temp3, temp4, temp5);
        return new SelectProvider7<T1, T2, TTemp1, TTemp2, TTemp3, TTemp4, TTemp5>(Executor, SqlBuilder);
    }

    #endregion

    #region TypeSet
    public IExpSelectGroup<TGroup, TypeSet<T1, T2>> GroupBy<TGroup>(Expression<Func<TypeSet<T1, T2>, TGroup>> exp)
    {
        var flatExp = FlatTypeSet.Default.Flat(exp)!;
        return this.GroupByHandle<TGroup, TypeSet<T1, T2>>(flatExp);
    }
    public IExpSelect<T1, T2> OrderBy(Expression<Func<TypeSet<T1, T2>, object>> exp)
    {
        var flatExp = FlatTypeSet.Default.Flat(exp)!;
        this.OrderByHandle(flatExp, true);
        return this;
    }
    public IExpSelect<T1, T2> OrderByDesc(Expression<Func<TypeSet<T1, T2>, object>> exp)
    {
        var flatExp = FlatTypeSet.Default.Flat(exp)!;
        this.OrderByHandle(flatExp, false);
        return this;
    }
    public IExpSelect<T1, T2> Where(Expression<Func<TypeSet<T1, T2>, bool>> exp)
    {
        var flatExp = FlatTypeSet.Default.Flat(exp)!;
        this.WhereHandle(flatExp);
        return this;
    }
    public IExpSelect<T1, T2, TJoin> InnerJoin<TJoin>(Expression<Func<TypeSet<T1, T2, TJoin>, bool>> exp)
    {
        var flatExp = FlatTypeSet.Default.Flat(exp)!;
        this.JoinHandle<TJoin>(flatExp, ExpressionSql.TableLinkType.InnerJoin);
        return new SelectProvider3<T1, T2, TJoin>(Executor, SqlBuilder);
    }

    public IExpSelect<T1, T2, TJoin> LeftJoin<TJoin>(Expression<Func<TypeSet<T1, T2, TJoin>, bool>> exp)
    {
        var flatExp = FlatTypeSet.Default.Flat(exp)!;
        this.JoinHandle<TJoin>(flatExp, ExpressionSql.TableLinkType.LeftJoin);
        return new SelectProvider3<T1, T2, TJoin>(Executor, SqlBuilder);
    }
    public IExpSelect<T1, T2, TJoin> RightJoin<TJoin>(Expression<Func<TypeSet<T1, T2, TJoin>, bool>> exp)
    {
        var flatExp = FlatTypeSet.Default.Flat(exp)!;
        this.JoinHandle<TJoin>(flatExp, ExpressionSql.TableLinkType.RightJoin);
        return new SelectProvider3<T1, T2, TJoin>(Executor, SqlBuilder);
    }
    public IExpSelect<T1, T2, TJoin> InnerJoin<TJoin>(IExpSelect<TJoin> subQuery, Expression<Func<TypeSet<T1, T2, TJoin>, bool>> where)
    {
        var flatExp = FlatTypeSet.Default.Flat(where);
        this.JoinHandle<TJoin>(flatExp, ExpressionSql.TableLinkType.InnerJoin, subQuery);
        return new SelectProvider3<T1, T2, TJoin>(Executor, SqlBuilder);
    }

    public IExpSelect<T1, T2, TJoin> LeftJoin<TJoin>(IExpSelect<TJoin> subQuery, Expression<Func<TypeSet<T1, T2, TJoin>, bool>> where)
    {
        var flatExp = FlatTypeSet.Default.Flat(where);
        this.JoinHandle<TJoin>(flatExp, ExpressionSql.TableLinkType.LeftJoin, subQuery);
        return new SelectProvider3<T1, T2, TJoin>(Executor, SqlBuilder);
    }
    public IExpSelect<T1, T2, TJoin> RightJoin<TJoin>(IExpSelect<TJoin> subQuery, Expression<Func<TypeSet<T1, T2, TJoin>, bool>> where)
    {
        var flatExp = FlatTypeSet.Default.Flat(where);
        this.JoinHandle<TJoin>(flatExp, ExpressionSql.TableLinkType.RightJoin, subQuery);
        return new SelectProvider3<T1, T2, TJoin>(Executor, SqlBuilder);
    }
    public IEnumerable<TReturn> ToList<TReturn>(Expression<Func<TypeSet<T1, T2>, TReturn>> exp)
    {
        var flatExp = FlatTypeSet.Default.Flat(exp)!;
        this.HandleResult(flatExp, null);
        return this.InternalToList<TReturn>();
    }
    public Task<IList<TReturn>> ToListAsync<TReturn>(Expression<Func<TypeSet<T1, T2>, TReturn>> exp, CancellationToken cancellationToken = default)
    {
        var flatExp = FlatTypeSet.Default.Flat(exp)!;
        this.HandleResult(flatExp, null);
        return this.InternalToListAsync<TReturn>(cancellationToken);
    }
    public IEnumerable<TReturn> ToList<TReturn>(Expression<Func<TypeSet<T1, T2>, object>> exp)
    {
        var flatExp = FlatTypeSet.Default.Flat(exp)!;
        this.HandleResult(flatExp, null);
        return this.InternalToList<TReturn>();
    }
    public Task<IList<TReturn>> ToListAsync<TReturn>(Expression<Func<TypeSet<T1, T2>, object>> exp, CancellationToken cancellationToken = default)
    {
        var flatExp = FlatTypeSet.Default.Flat(exp)!;
        this.HandleResult(flatExp, null);
        return this.InternalToListAsync<TReturn>(cancellationToken);
    }

    public IAsyncEnumerable<TReturn> ToEnumerableAsync<TReturn>(Expression<Func<TypeSet<T1, T2>, TReturn>> exp, CancellationToken cancellationToken = default)
    {
        var flatExp = FlatTypeSet.Default.Flat(exp)!;
        this.HandleResult(flatExp, null);
        return this.InternalToEnumerableAsync<TReturn>(cancellationToken);
    }

    public IAsyncEnumerable<TReturn> ToEnumerableAsync<TReturn>(Expression<Func<TypeSet<T1, T2>, object>> exp, CancellationToken cancellationToken = default)
    {
        var flatExp = FlatTypeSet.Default.Flat(exp)!;
        this.HandleResult(flatExp, null);
        return this.InternalToEnumerableAsync<TReturn>(cancellationToken);
    }


    //public IExpSelect<TTemp> AsSubQuery<TTemp>(Expression<Func<TypeSet<T1, T2>, TTemp>> exp, string? alias = null)
    //{
    //    var flatExp = FlatTypeSet.Default.Flat(exp)!;
    //    this.HandleResult(flatExp, null);
    //    return this.HandleSubQuery<TTemp>(alias);
    //}

    public IExpSelect<TTable> AsTable<TTable>(Expression<Func<TypeSet<T1, T2>, TTable>> exp)
    {
        var flatExp = FlatTypeSet.Default.Flat(exp);
        this.HandleResult(flatExp, null);
        return new SelectProvider1<TTable>(Executor, SqlBuilder);
    }

    public IExpTemp<TTemp> AsTemp<TTemp>(string name, Expression<Func<TypeSet<T1, T2>, TTemp>> exp)
    {
        var flatExp = FlatTypeSet.Default.Flat(exp)!;
        this.HandleResult(flatExp, null);
        return new TempProvider<TTemp>(name, SqlBuilder);
    }

    public string ToSql(Expression<Func<TypeSet<T1, T2>, object>> exp)
    {
        var flatExp = FlatTypeSet.Default.Flat(exp)!;
        this.HandleResult(flatExp, null);
        return ToSql();
    }

    #endregion


}
//#if NET45_OR_GREATER
//internal sealed class SelectProvider3<T1, T2, T3> : SelectProvider0<IExpSelect<T1, T2, T3>, T1>, IExpSelect<T1, T2, T3>
//{
//    public SelectProvider3(Expression exp, ISqlExecutor executor) : base(executor)
//    {
//        SelectExpression = new ExpressionInfo()
//        {
//            Expression = exp,
//            ResolveOptions = SqlResolveOptions.Select
//        };
//        SqlBuilder.Expressions.Add(SelectExpression);
//    }

//    public IExpSelect<T1, T2, T3> InnerJoin<TJoin>(Expression<Func<TypeSet<T1, T2, T3>, bool>> exp)
//    {
//        return JoinHandle<TJoin>(exp, ExpressionSql.TableLinkType.InnerJoin);
//    }

//    public IExpSelect<T1, T2, T3> LeftJoin<TJoin>(Expression<Func<TypeSet<T1, T2, T3>, bool>> exp)
//    {
//        return JoinHandle<TJoin>(exp, ExpressionSql.TableLinkType.LeftJoin);
//    }
//    public IExpSelect<T1, T2, T3> RightJoin<TJoin>(Expression<Func<TypeSet<T1, T2, T3>, bool>> exp)
//    {
//        return JoinHandle<TJoin>(exp, ExpressionSql.TableLinkType.RightJoin);
//    }

//    public IExpSelect<T1, T2, T3> GroupBy(Expression<Func<TypeSet<T1, T2, T3>, object>> exp)
//    {
//        return GroupByHandle(exp);
//    }

//    public IExpSelect<T1, T2, T3> OrderBy(Expression<Func<TypeSet<T1, T2, T3>, object>> exp, bool asc = true)
//    {
//        return OrderByHandle(exp, asc);
//    }

//    public IExpSelect<T1, T2, T3> Where(Expression<Func<TypeSet<T1, T2, T3>, bool>> exp)
//    {
//        return WhereHandle(exp);
//    }
//}

//internal sealed class SelectProvider4<T1, T2, T3, T4> : SelectProvider0<IExpSelect<T1, T2, T3, T4>, T1>, IExpSelect<T1, T2, T3, T4>
//{
//    public SelectProvider4(Expression exp, ISqlExecutor executor) : base(executor)
//    {
//        SelectExpression = new ExpressionInfo()
//        {
//            Expression = exp,
//            ResolveOptions = SqlResolveOptions.Select
//        };
//        SqlBuilder.Expressions.Add(SelectExpression);
//    }

//    public IExpSelect<T1, T2, T3, T4> InnerJoin<TJoin>(Expression<Func<TypeSet<T1, T2, T3, T4>, bool>> exp)
//    {
//        return JoinHandle<TJoin>(exp, ExpressionSql.TableLinkType.InnerJoin);
//    }

//    public IExpSelect<T1, T2, T3, T4> LeftJoin<TJoin>(Expression<Func<TypeSet<T1, T2, T3, T4>, bool>> exp)
//    {
//        return JoinHandle<TJoin>(exp, ExpressionSql.TableLinkType.LeftJoin);
//    }
//    public IExpSelect<T1, T2, T3, T4> RightJoin<TJoin>(Expression<Func<TypeSet<T1, T2, T3, T4>, bool>> exp)
//    {
//        return JoinHandle<TJoin>(exp, ExpressionSql.TableLinkType.RightJoin);
//    }

//    public IExpSelect<T1, T2, T3, T4> GroupBy(Expression<Func<TypeSet<T1, T2, T3, T4>, object>> exp)
//    {
//        return GroupByHandle(exp);
//    }

//    public IExpSelect<T1, T2, T3, T4> OrderBy(Expression<Func<TypeSet<T1, T2, T3, T4>, object>> exp, bool asc = true)
//    {
//        return OrderByHandle(exp, asc);
//    }

//    public IExpSelect<T1, T2, T3, T4> Where(Expression<Func<TypeSet<T1, T2, T3, T4>, bool>> exp)
//    {
//        return WhereHandle(exp);
//    }
//}

//internal sealed class SelectProvider5<T1, T2, T3, T4, T5> : SelectProvider0<IExpSelect<T1, T2, T3, T4, T5>, T1>, IExpSelect<T1, T2, T3, T4, T5>
//{
//    public SelectProvider5(Expression exp, ISqlExecutor executor) : base(executor)
//    {
//        SelectExpression = new ExpressionInfo()
//        {
//            Expression = exp,
//            ResolveOptions = SqlResolveOptions.Select
//        };
//        SqlBuilder.Expressions.Add(SelectExpression);
//    }

//    public IExpSelect<T1, T2, T3, T4, T5> InnerJoin<TJoin>(Expression<Func<TypeSet<T1, T2, T3, T4, T5>, bool>> exp)
//    {
//        return JoinHandle<TJoin>(exp, ExpressionSql.TableLinkType.InnerJoin);
//    }

//    public IExpSelect<T1, T2, T3, T4, T5> LeftJoin<TJoin>(Expression<Func<TypeSet<T1, T2, T3, T4, T5>, bool>> exp)
//    {
//        return JoinHandle<TJoin>(exp, ExpressionSql.TableLinkType.LeftJoin);
//    }
//    public IExpSelect<T1, T2, T3, T4, T5> RightJoin<TJoin>(Expression<Func<TypeSet<T1, T2, T3, T4, T5>, bool>> exp)
//    {
//        return JoinHandle<TJoin>(exp, ExpressionSql.TableLinkType.RightJoin);
//    }

//    public IExpSelect<T1, T2, T3, T4, T5> GroupBy(Expression<Func<TypeSet<T1, T2, T3, T4, T5>, object>> exp)
//    {
//        return GroupByHandle(exp);
//    }

//    public IExpSelect<T1, T2, T3, T4, T5> OrderBy(Expression<Func<TypeSet<T1, T2, T3, T4, T5>, object>> exp, bool asc = true)
//    {
//        return OrderByHandle(exp, asc);
//    }

//    public IExpSelect<T1, T2, T3, T4, T5> Where(Expression<Func<TypeSet<T1, T2, T3, T4, T5>, bool>> exp)
//    {
//        return WhereHandle(exp);
//    }
//}

//internal sealed class SelectProvider6<T1, T2, T3, T4, T5, T6> : SelectProvider0<IExpSelect<T1, T2, T3, T4, T5, T6>, T1>, IExpSelect<T1, T2, T3, T4, T5, T6>
//{
//    public SelectProvider6(Expression exp, ISqlExecutor executor) : base(executor)
//    {
//        SelectExpression = new ExpressionInfo()
//        {
//            Expression = exp,
//            ResolveOptions = SqlResolveOptions.Select
//        };
//        SqlBuilder.Expressions.Add(SelectExpression);
//    }

//    public IExpSelect<T1, T2, T3, T4, T5, T6> InnerJoin<TJoin>(Expression<Func<TypeSet<T1, T2, T3, T4, T5, T6>, bool>> exp)
//    {
//        return JoinHandle<TJoin>(exp, ExpressionSql.TableLinkType.InnerJoin);
//    }

//    public IExpSelect<T1, T2, T3, T4, T5, T6> LeftJoin<TJoin>(Expression<Func<TypeSet<T1, T2, T3, T4, T5, T6>, bool>> exp)
//    {
//        return JoinHandle<TJoin>(exp, ExpressionSql.TableLinkType.LeftJoin);
//    }
//    public IExpSelect<T1, T2, T3, T4, T5, T6> RightJoin<TJoin>(Expression<Func<TypeSet<T1, T2, T3, T4, T5, T6>, bool>> exp)
//    {
//        return JoinHandle<TJoin>(exp, ExpressionSql.TableLinkType.RightJoin);
//    }

//    public IExpSelect<T1, T2, T3, T4, T5, T6> GroupBy(Expression<Func<TypeSet<T1, T2, T3, T4, T5, T6>, object>> exp)
//    {
//        return GroupByHandle(exp);
//    }

//    public IExpSelect<T1, T2, T3, T4, T5, T6> OrderBy(Expression<Func<TypeSet<T1, T2, T3, T4, T5, T6>, object>> exp, bool asc = true)
//    {
//        return OrderByHandle(exp, asc);
//    }

//    public IExpSelect<T1, T2, T3, T4, T5, T6> Where(Expression<Func<TypeSet<T1, T2, T3, T4, T5, T6>, bool>> exp)
//    {
//        return WhereHandle(exp);
//    }
//}

//internal sealed class SelectProvider7<T1, T2, T3, T4, T5, T6, T7> : SelectProvider0<IExpSelect<T1, T2, T3, T4, T5, T6, T7>, T1>, IExpSelect<T1, T2, T3, T4, T5, T6, T7>
//{
//    public SelectProvider7(Expression exp, ISqlExecutor executor) : base(executor)
//    {
//        SelectExpression = new ExpressionInfo()
//        {
//            Expression = exp,
//            ResolveOptions = SqlResolveOptions.Select
//        };
//        SqlBuilder.Expressions.Add(SelectExpression);
//    }

//    public IExpSelect<T1, T2, T3, T4, T5, T6, T7> InnerJoin<TJoin>(Expression<Func<TypeSet<T1, T2, T3, T4, T5, T6, T7>, bool>> exp)
//    {
//        return JoinHandle<TJoin>(exp, ExpressionSql.TableLinkType.InnerJoin);
//    }

//    public IExpSelect<T1, T2, T3, T4, T5, T6, T7> LeftJoin<TJoin>(Expression<Func<TypeSet<T1, T2, T3, T4, T5, T6, T7>, bool>> exp)
//    {
//        return JoinHandle<TJoin>(exp, ExpressionSql.TableLinkType.LeftJoin);
//    }
//    public IExpSelect<T1, T2, T3, T4, T5, T6, T7> RightJoin<TJoin>(Expression<Func<TypeSet<T1, T2, T3, T4, T5, T6, T7>, bool>> exp)
//    {
//        return JoinHandle<TJoin>(exp, ExpressionSql.TableLinkType.RightJoin);
//    }

//    public IExpSelect<T1, T2, T3, T4, T5, T6, T7> GroupBy(Expression<Func<TypeSet<T1, T2, T3, T4, T5, T6, T7>, object>> exp)
//    {
//        return GroupByHandle(exp);
//    }

//    public IExpSelect<T1, T2, T3, T4, T5, T6, T7> OrderBy(Expression<Func<TypeSet<T1, T2, T3, T4, T5, T6, T7>, object>> exp, bool asc = true)
//    {
//        return OrderByHandle(exp, asc);
//    }

//    public IExpSelect<T1, T2, T3, T4, T5, T6, T7> Where(Expression<Func<TypeSet<T1, T2, T3, T4, T5, T6, T7>, bool>> exp)
//    {
//        return WhereHandle(exp);
//    }
//}

//internal sealed class SelectProvider8<T1, T2, T3, T4, T5, T6, T7, T8> : SelectProvider0<IExpSelect<T1, T2, T3, T4, T5, T6, T7, T8>, T1>, IExpSelect<T1, T2, T3, T4, T5, T6, T7, T8>
//{
//    public SelectProvider8(Expression exp, ISqlExecutor executor) : base(executor)
//    {
//        SelectExpression = new ExpressionInfo()
//        {
//            Expression = exp,
//            ResolveOptions = SqlResolveOptions.Select
//        };
//        SqlBuilder.Expressions.Add(SelectExpression);
//    }

//    public IExpSelect<T1, T2, T3, T4, T5, T6, T7, T8> InnerJoin<TJoin>(Expression<Func<TypeSet<T1, T2, T3, T4, T5, T6, T7, T8>, bool>> exp)
//    {
//        return JoinHandle<TJoin>(exp, ExpressionSql.TableLinkType.InnerJoin);
//    }

//    public IExpSelect<T1, T2, T3, T4, T5, T6, T7, T8> LeftJoin<TJoin>(Expression<Func<TypeSet<T1, T2, T3, T4, T5, T6, T7, T8>, bool>> exp)
//    {
//        return JoinHandle<TJoin>(exp, ExpressionSql.TableLinkType.LeftJoin);
//    }
//    public IExpSelect<T1, T2, T3, T4, T5, T6, T7, T8> RightJoin<TJoin>(Expression<Func<TypeSet<T1, T2, T3, T4, T5, T6, T7, T8>, bool>> exp)
//    {
//        return JoinHandle<TJoin>(exp, ExpressionSql.TableLinkType.RightJoin);
//    }

//    public IExpSelect<T1, T2, T3, T4, T5, T6, T7, T8> GroupBy(Expression<Func<TypeSet<T1, T2, T3, T4, T5, T6, T7, T8>, object>> exp)
//    {
//        return GroupByHandle(exp);
//    }

//    public IExpSelect<T1, T2, T3, T4, T5, T6, T7, T8> OrderBy(Expression<Func<TypeSet<T1, T2, T3, T4, T5, T6, T7, T8>, object>> exp, bool asc = true)
//    {
//        return OrderByHandle(exp, asc);
//    }

//    public IExpSelect<T1, T2, T3, T4, T5, T6, T7, T8> Where(Expression<Func<TypeSet<T1, T2, T3, T4, T5, T6, T7, T8>, bool>> exp)
//    {
//        return WhereHandle(exp);
//    }
//}

//internal sealed class SelectProvider9<T1, T2, T3, T4, T5, T6, T7, T8, T9> : SelectProvider0<IExpSelect<T1, T2, T3, T4, T5, T6, T7, T8, T9>, T1>, IExpSelect<T1, T2, T3, T4, T5, T6, T7, T8, T9>
//{
//    public SelectProvider9(Expression exp, ISqlExecutor executor) : base(executor)
//    {
//        SelectExpression = new ExpressionInfo()
//        {
//            Expression = exp,
//            ResolveOptions = SqlResolveOptions.Select
//        };
//        SqlBuilder.Expressions.Add(SelectExpression);
//    }

//    public IExpSelect<T1, T2, T3, T4, T5, T6, T7, T8, T9> InnerJoin<TJoin>(Expression<Func<TypeSet<T1, T2, T3, T4, T5, T6, T7, T8, T9>, bool>> exp)
//    {
//        return JoinHandle<TJoin>(exp, ExpressionSql.TableLinkType.InnerJoin);
//    }

//    public IExpSelect<T1, T2, T3, T4, T5, T6, T7, T8, T9> LeftJoin<TJoin>(Expression<Func<TypeSet<T1, T2, T3, T4, T5, T6, T7, T8, T9>, bool>> exp)
//    {
//        return JoinHandle<TJoin>(exp, ExpressionSql.TableLinkType.LeftJoin);
//    }
//    public IExpSelect<T1, T2, T3, T4, T5, T6, T7, T8, T9> RightJoin<TJoin>(Expression<Func<TypeSet<T1, T2, T3, T4, T5, T6, T7, T8, T9>, bool>> exp)
//    {
//        return JoinHandle<TJoin>(exp, ExpressionSql.TableLinkType.RightJoin);
//    }

//    public IExpSelect<T1, T2, T3, T4, T5, T6, T7, T8, T9> GroupBy(Expression<Func<TypeSet<T1, T2, T3, T4, T5, T6, T7, T8, T9>, object>> exp)
//    {
//        return GroupByHandle(exp);
//    }

//    public IExpSelect<T1, T2, T3, T4, T5, T6, T7, T8, T9> OrderBy(Expression<Func<TypeSet<T1, T2, T3, T4, T5, T6, T7, T8, T9>, object>> exp, bool asc = true)
//    {
//        return OrderByHandle(exp, asc);
//    }

//    public IExpSelect<T1, T2, T3, T4, T5, T6, T7, T8, T9> Where(Expression<Func<TypeSet<T1, T2, T3, T4, T5, T6, T7, T8, T9>, bool>> exp)
//    {
//        return WhereHandle(exp);
//    }
//}

//internal sealed class SelectProvider10<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> : SelectProvider0<IExpSelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>, T1>, IExpSelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>
//{
//    public SelectProvider10(Expression exp, ISqlExecutor executor) : base(executor)
//    {
//        SelectExpression = new ExpressionInfo()
//        {
//            Expression = exp,
//            ResolveOptions = SqlResolveOptions.Select
//        };
//        SqlBuilder.Expressions.Add(SelectExpression);
//    }

//    public IExpSelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> InnerJoin<TJoin>(Expression<Func<TypeSet<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>, bool>> exp)
//    {
//        return JoinHandle<TJoin>(exp, ExpressionSql.TableLinkType.InnerJoin);
//    }

//    public IExpSelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> LeftJoin<TJoin>(Expression<Func<TypeSet<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>, bool>> exp)
//    {
//        return JoinHandle<TJoin>(exp, ExpressionSql.TableLinkType.LeftJoin);
//    }
//    public IExpSelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> RightJoin<TJoin>(Expression<Func<TypeSet<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>, bool>> exp)
//    {
//        return JoinHandle<TJoin>(exp, ExpressionSql.TableLinkType.RightJoin);
//    }

//    public IExpSelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> GroupBy(Expression<Func<TypeSet<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>, object>> exp)
//    {
//        return GroupByHandle(exp);
//    }

//    public IExpSelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> OrderBy(Expression<Func<TypeSet<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>, object>> exp, bool asc = true)
//    {
//        return OrderByHandle(exp, asc);
//    }

//    public IExpSelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> Where(Expression<Func<TypeSet<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>, bool>> exp)
//    {
//        return WhereHandle(exp);
//    }
//}

//internal sealed class SelectProvider11<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> : SelectProvider0<IExpSelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>, T1>, IExpSelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>
//{
//    public SelectProvider11(Expression exp, ISqlExecutor executor) : base(executor)
//    {
//        SelectExpression = new ExpressionInfo()
//        {
//            Expression = exp,
//            ResolveOptions = SqlResolveOptions.Select
//        };
//        SqlBuilder.Expressions.Add(SelectExpression);
//    }

//    public IExpSelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> InnerJoin<TJoin>(Expression<Func<TypeSet<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>, bool>> exp)
//    {
//        return JoinHandle<TJoin>(exp, ExpressionSql.TableLinkType.InnerJoin);
//    }

//    public IExpSelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> LeftJoin<TJoin>(Expression<Func<TypeSet<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>, bool>> exp)
//    {
//        return JoinHandle<TJoin>(exp, ExpressionSql.TableLinkType.LeftJoin);
//    }
//    public IExpSelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> RightJoin<TJoin>(Expression<Func<TypeSet<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>, bool>> exp)
//    {
//        return JoinHandle<TJoin>(exp, ExpressionSql.TableLinkType.RightJoin);
//    }

//    public IExpSelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> GroupBy(Expression<Func<TypeSet<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>, object>> exp)
//    {
//        return GroupByHandle(exp);
//    }

//    public IExpSelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> OrderBy(Expression<Func<TypeSet<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>, object>> exp, bool asc = true)
//    {
//        return OrderByHandle(exp, asc);
//    }

//    public IExpSelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> Where(Expression<Func<TypeSet<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>, bool>> exp)
//    {
//        return WhereHandle(exp);
//    }
//}

//internal sealed class SelectProvider12<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> : SelectProvider0<IExpSelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>, T1>, IExpSelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>
//{
//    public SelectProvider12(Expression exp, ISqlExecutor executor) : base(executor)
//    {
//        SelectExpression = new ExpressionInfo()
//        {
//            Expression = exp,
//            ResolveOptions = SqlResolveOptions.Select
//        };
//        SqlBuilder.Expressions.Add(SelectExpression);
//    }

//    public IExpSelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> InnerJoin<TJoin>(Expression<Func<TypeSet<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>, bool>> exp)
//    {
//        return JoinHandle<TJoin>(exp, ExpressionSql.TableLinkType.InnerJoin);
//    }

//    public IExpSelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> LeftJoin<TJoin>(Expression<Func<TypeSet<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>, bool>> exp)
//    {
//        return JoinHandle<TJoin>(exp, ExpressionSql.TableLinkType.LeftJoin);
//    }
//    public IExpSelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> RightJoin<TJoin>(Expression<Func<TypeSet<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>, bool>> exp)
//    {
//        return JoinHandle<TJoin>(exp, ExpressionSql.TableLinkType.RightJoin);
//    }

//    public IExpSelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> GroupBy(Expression<Func<TypeSet<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>, object>> exp)
//    {
//        return GroupByHandle(exp);
//    }

//    public IExpSelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> OrderBy(Expression<Func<TypeSet<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>, object>> exp, bool asc = true)
//    {
//        return OrderByHandle(exp, asc);
//    }

//    public IExpSelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> Where(Expression<Func<TypeSet<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>, bool>> exp)
//    {
//        return WhereHandle(exp);
//    }
//}

//internal sealed class SelectProvider13<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> : SelectProvider0<IExpSelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>, T1>, IExpSelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>
//{
//    public SelectProvider13(Expression exp, ISqlExecutor executor) : base(executor)
//    {
//        SelectExpression = new ExpressionInfo()
//        {
//            Expression = exp,
//            ResolveOptions = SqlResolveOptions.Select
//        };
//        SqlBuilder.Expressions.Add(SelectExpression);
//    }

//    public IExpSelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> InnerJoin<TJoin>(Expression<Func<TypeSet<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>, bool>> exp)
//    {
//        return JoinHandle<TJoin>(exp, ExpressionSql.TableLinkType.InnerJoin);
//    }

//    public IExpSelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> LeftJoin<TJoin>(Expression<Func<TypeSet<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>, bool>> exp)
//    {
//        return JoinHandle<TJoin>(exp, ExpressionSql.TableLinkType.LeftJoin);
//    }
//    public IExpSelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> RightJoin<TJoin>(Expression<Func<TypeSet<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>, bool>> exp)
//    {
//        return JoinHandle<TJoin>(exp, ExpressionSql.TableLinkType.RightJoin);
//    }

//    public IExpSelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> GroupBy(Expression<Func<TypeSet<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>, object>> exp)
//    {
//        return GroupByHandle(exp);
//    }

//    public IExpSelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> OrderBy(Expression<Func<TypeSet<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>, object>> exp, bool asc = true)
//    {
//        return OrderByHandle(exp, asc);
//    }

//    public IExpSelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> Where(Expression<Func<TypeSet<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>, bool>> exp)
//    {
//        return WhereHandle(exp);
//    }
//}

//internal sealed class SelectProvider14<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> : SelectProvider0<IExpSelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>, T1>, IExpSelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>
//{
//    public SelectProvider14(Expression exp, ISqlExecutor executor) : base(executor)
//    {
//        SelectExpression = new ExpressionInfo()
//        {
//            Expression = exp,
//            ResolveOptions = SqlResolveOptions.Select
//        };
//        SqlBuilder.Expressions.Add(SelectExpression);
//    }

//    public IExpSelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> InnerJoin<TJoin>(Expression<Func<TypeSet<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>, bool>> exp)
//    {
//        return JoinHandle<TJoin>(exp, ExpressionSql.TableLinkType.InnerJoin);
//    }

//    public IExpSelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> LeftJoin<TJoin>(Expression<Func<TypeSet<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>, bool>> exp)
//    {
//        return JoinHandle<TJoin>(exp, ExpressionSql.TableLinkType.LeftJoin);
//    }
//    public IExpSelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> RightJoin<TJoin>(Expression<Func<TypeSet<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>, bool>> exp)
//    {
//        return JoinHandle<TJoin>(exp, ExpressionSql.TableLinkType.RightJoin);
//    }

//    public IExpSelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> GroupBy(Expression<Func<TypeSet<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>, object>> exp)
//    {
//        return GroupByHandle(exp);
//    }

//    public IExpSelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> OrderBy(Expression<Func<TypeSet<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>, object>> exp, bool asc = true)
//    {
//        return OrderByHandle(exp, asc);
//    }

//    public IExpSelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> Where(Expression<Func<TypeSet<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>, bool>> exp)
//    {
//        return WhereHandle(exp);
//    }
//}

//internal sealed class SelectProvider15<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> : SelectProvider0<IExpSelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>, T1>, IExpSelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>
//{
//    public SelectProvider15(Expression exp, ISqlExecutor executor) : base(executor)
//    {
//        SelectExpression = new ExpressionInfo()
//        {
//            Expression = exp,
//            ResolveOptions = SqlResolveOptions.Select
//        };
//        SqlBuilder.Expressions.Add(SelectExpression);
//    }

//    public IExpSelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> InnerJoin<TJoin>(Expression<Func<TypeSet<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>, bool>> exp)
//    {
//        return JoinHandle<TJoin>(exp, ExpressionSql.TableLinkType.InnerJoin);
//    }

//    public IExpSelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> LeftJoin<TJoin>(Expression<Func<TypeSet<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>, bool>> exp)
//    {
//        return JoinHandle<TJoin>(exp, ExpressionSql.TableLinkType.LeftJoin);
//    }
//    public IExpSelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> RightJoin<TJoin>(Expression<Func<TypeSet<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>, bool>> exp)
//    {
//        return JoinHandle<TJoin>(exp, ExpressionSql.TableLinkType.RightJoin);
//    }

//    public IExpSelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> GroupBy(Expression<Func<TypeSet<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>, object>> exp)
//    {
//        return GroupByHandle(exp);
//    }

//    public IExpSelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> OrderBy(Expression<Func<TypeSet<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>, object>> exp, bool asc = true)
//    {
//        return OrderByHandle(exp, asc);
//    }

//    public IExpSelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> Where(Expression<Func<TypeSet<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>, bool>> exp)
//    {
//        return WhereHandle(exp);
//    }
//}

//internal sealed class SelectProvider16<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> : SelectProvider0<IExpSelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>, T1>, IExpSelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>
//{
//    public SelectProvider16(Expression exp, ISqlExecutor executor) : base(executor)
//    {
//        SelectExpression = new ExpressionInfo()
//        {
//            Expression = exp,
//            ResolveOptions = SqlResolveOptions.Select
//        };
//        SqlBuilder.Expressions.Add(SelectExpression);
//    }

//    public IExpSelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> InnerJoin<TJoin>(Expression<Func<TypeSet<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>, bool>> exp)
//    {
//        return JoinHandle<TJoin>(exp, ExpressionSql.TableLinkType.InnerJoin);
//    }

//    public IExpSelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> LeftJoin<TJoin>(Expression<Func<TypeSet<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>, bool>> exp)
//    {
//        return JoinHandle<TJoin>(exp, ExpressionSql.TableLinkType.LeftJoin);
//    }
//    public IExpSelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> RightJoin<TJoin>(Expression<Func<TypeSet<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>, bool>> exp)
//    {
//        return JoinHandle<TJoin>(exp, ExpressionSql.TableLinkType.RightJoin);
//    }

//    public IExpSelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> GroupBy(Expression<Func<TypeSet<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>, object>> exp)
//    {
//        return GroupByHandle(exp);
//    }

//    public IExpSelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> OrderBy(Expression<Func<TypeSet<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>, object>> exp, bool asc = true)
//    {
//        return OrderByHandle(exp, asc);
//    }

//    public IExpSelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> Where(Expression<Func<TypeSet<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>, bool>> exp)
//    {
//        return WhereHandle(exp);
//    }
//}
//#endif
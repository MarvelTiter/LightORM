using LightORM.Extension;
using LightORM.Interfaces.ExpSql;
namespace LightORM.Providers.Select;

//TODO Select 匿名类
internal sealed class SelectProvider0 : SelectProvider0<IExpSelect<object>, object>, IExpSelect
{
    public SelectProvider0(ISqlExecutor executor) : base(executor)
    {
        //SqlBuilder.TableInfo = new TableEntity
        //{
        //    CustomName = table,
        //    IsAnonymousType = true,
        //};
    }
}

internal class SelectProvider1<T1> : SelectProvider0<IExpSelect<T1>, T1>, IExpSelect<T1>
{
    public SelectProvider1(ISqlExecutor executor, SelectBuilder? builder = null)
        : base(executor, builder)
    {
        if (builder == null)
        {
            SqlBuilder = new SelectBuilder(DbType);
            SqlBuilder.SelectedTables.Add(TableContext.GetTableInfo<T1>());
        }
    }
    public IExpSelect<T1> As(string alias)
    {
        SqlBuilder.MainTable.Alias = alias;
        return this;
    }

    public IExpSelect<T1> Union(IExpSelect<T1> select)
    {
        var provider = this.HandleSubQuery<T1>();
        SqlBuilder.AddUnion(select.SqlBuilder, false);
        return provider;
    }
    public IExpSelect<T1> UnionAll(IExpSelect<T1> select)
    {
        var provider = this.HandleSubQuery<T1>();
        SqlBuilder.AddUnion(select.SqlBuilder, true);
        return provider;
    }

    public IExpSelectGroup<TGroup, T1> GroupBy<TGroup>(Expression<Func<T1, TGroup>> exp)
    {
        return this.GroupByHandle<TGroup, T1>(exp);
    }
    public IExpSelect<T1> OrderBy(Expression<Func<T1, object>> exp)
    {
        this.OrderByHandle(exp, true);
        return this;
    }
    public IExpSelect<T1> OrderByDesc(Expression<Func<T1, object>> exp)
    {
        this.OrderByHandle(exp, false);
        return this;
    }

    #region Join

    public IExpSelect<T1, TJoin> InnerJoin<TJoin>(Expression<Func<T1, TJoin, bool>> exp)
    {
        this.JoinHandle<TJoin>(exp, ExpressionSql.TableLinkType.InnerJoin);
        return new SelectProvider2<T1, TJoin>(Executor, SqlBuilder);
    }

    public IExpSelect<T1, TJoin> LeftJoin<TJoin>(Expression<Func<T1, TJoin, bool>> exp)
    {
        this.JoinHandle<TJoin>(exp, ExpressionSql.TableLinkType.LeftJoin);
        return new SelectProvider2<T1, TJoin>(Executor, SqlBuilder);
    }

    public IExpSelect<T1, TJoin> RightJoin<TJoin>(Expression<Func<T1, TJoin, bool>> exp)
    {
        this.JoinHandle<TJoin>(exp, ExpressionSql.TableLinkType.RightJoin);
        return new SelectProvider2<T1, TJoin>(Executor, SqlBuilder);
    }

    public IExpSelect<T1, TJoin> InnerJoin<TJoin>(IExpSelect<TJoin> subQuery, Expression<Func<T1, TJoin, bool>> exp)
    {
        this.JoinHandle<TJoin>(exp, ExpressionSql.TableLinkType.InnerJoin, subQuery);
        return new SelectProvider2<T1, TJoin>(Executor, SqlBuilder);
    }

    public IExpSelect<T1, TJoin> LeftJoin<TJoin>(IExpSelect<TJoin> subQuery, Expression<Func<T1, TJoin, bool>> exp)
    {
        this.JoinHandle<TJoin>(exp, ExpressionSql.TableLinkType.LeftJoin, subQuery);
        return new SelectProvider2<T1, TJoin>(Executor, SqlBuilder);
    }

    public IExpSelect<T1, TJoin> RightJoin<TJoin>(IExpSelect<TJoin> subQuery, Expression<Func<T1, TJoin, bool>> exp)
    {
        this.JoinHandle<TJoin>(exp, ExpressionSql.TableLinkType.RightJoin, subQuery);
        return new SelectProvider2<T1, TJoin>(Executor, SqlBuilder);
    }

    #endregion

    public IEnumerable<TReturn> ToList<TReturn>(Expression<Func<T1, TReturn>> exp)
    {
        this.HandleResult(exp, null);
        return ToList<TReturn>();
    }
    public Task<IList<TReturn>> ToListAsync<TReturn>(Expression<Func<T1, TReturn>> exp)
    {
        this.HandleResult(exp, null);
        return ToListAsync<TReturn>();
    }
    public IEnumerable<dynamic> ToDynamicList(Expression<Func<T1, object>> exp)
    {
        this.HandleResult(exp, null);
        return ToList<MapperRow>();
    }

    public async Task<IList<dynamic>> ToDynamicListAsync(Expression<Func<T1, object>> exp)
    {
        this.HandleResult(exp, null);
        var list = await ToListAsync<MapperRow>();
        return list.Cast<dynamic>().ToList();
    }
    public IExpInclude<T1, TMember> Include<TMember>(Expression<Func<T1, TMember>> exp)
    {
        return CreateIncludeProvider<TMember>(exp);
    }

    public string ToSql(Expression<Func<T1, object>> exp)
    {
        this.HandleResult(exp, null);
        return ToSql();
    }

    internal IExpInclude<T1, TMember> CreateIncludeProvider<TMember>(Expression exp)
    {
        var option = SqlResolveOptions.Select;
        var result = exp.Resolve(option, ResolveContext.Create(Executor.ConnectInfo.DbBaseType));
        var navName = result.NavigateMembers!.First();
        var navCol = SqlBuilder.MainTable.GetColumnInfo(navName);
        var navInfo = navCol.NavigateInfo!;
        var table = TableContext.GetTableInfo(navCol.NavigateInfo!.NavigateType);
        var parentWhereColumn = SqlBuilder.MainTable.GetColumnInfo(navCol.NavigateInfo!.MainName!);
        var includeInfo = new IncludeInfo
        {
            SelectedTable = table,
            NavigateInfo = navInfo,
            ParentNavigateColumn = navCol,
            ParentWhereColumn = parentWhereColumn,
            ExpressionResolvedResult = result
        };
        SqlBuilder.IncludeContext.Includes.Add(includeInfo);
        return new IncludeProvider<T1, TMember>(Executor, SqlBuilder);
    }

    public IExpSelect<TTemp> AsSubQuery<TTemp>(Expression<Func<T1, TTemp>> exp)
    {
        this.HandleResult(exp, null);
        return this.HandleSubQuery<TTemp>();
    }

    public IExpTemp<TTemp> AsTemp<TTemp>(string name, Expression<Func<T1, TTemp>> exp)
    {
        this.HandleResult(exp, null);
        return new TempProvider<TTemp>(name, SqlBuilder);
    }


    #region with temp
    public IExpSelect<T1, TTemp> WithTempQuery<TTemp>(IExpTemp<TTemp> temp)
    {
        //SqlBuilder.TempViews.Add(temp.SqlBuilder);
        //SqlBuilder.SelectedTables.Add(temp.ResultTable);
        this.HandleTempQuery(temp);
        return new SelectProvider2<T1, TTemp>(Executor, SqlBuilder);
    }

    public IExpSelect<T1, TTemp1, TTemp2> WithTempQuery<TTemp1, TTemp2>(IExpTemp<TTemp1> temp1, IExpTemp<TTemp2> temp2)
    {
        //SqlBuilder.TempViews.Add(temp1.SqlBuilder);
        //SqlBuilder.TempViews.Add(temp2.SqlBuilder);
        //SqlBuilder.SelectedTables.Add(temp1.ResultTable);
        //SqlBuilder.SelectedTables.Add(temp2.ResultTable);
        this.HandleTempQuery(temp1, temp2);
        return new SelectProvider3<T1, TTemp1, TTemp2>(Executor, SqlBuilder);
    }

    public IExpSelect<T1, TTemp1, TTemp2, TTemp3> WithTempQuery<TTemp1, TTemp2, TTemp3>(IExpTemp<TTemp1> temp1, IExpTemp<TTemp2> temp2, IExpTemp<TTemp3> temp3)
    {
        this.HandleTempQuery(temp1, temp2, temp3);
        return new SelectProvider4<T1, TTemp1, TTemp2, TTemp3>(Executor, SqlBuilder);
    }

    public IExpSelect<T1, TTemp1, TTemp2, TTemp3, TTemp4> WithTempQuery<TTemp1, TTemp2, TTemp3, TTemp4>(IExpTemp<TTemp1> temp1, IExpTemp<TTemp2> temp2, IExpTemp<TTemp3> temp3, IExpTemp<TTemp4> temp4)
    {
        this.HandleTempQuery(temp1, temp2, temp3, temp4);
        return new SelectProvider5<T1, TTemp1, TTemp2, TTemp3, TTemp4>(Executor, SqlBuilder);
    }

    public IExpSelect<T1, TTemp1, TTemp2, TTemp3, TTemp4, TTemp5> WithTempQuery<TTemp1, TTemp2, TTemp3, TTemp4, TTemp5>(IExpTemp<TTemp1> temp1, IExpTemp<TTemp2> temp2, IExpTemp<TTemp3> temp3, IExpTemp<TTemp4> temp4, IExpTemp<TTemp5> temp5)
    {
        this.HandleTempQuery(temp1, temp2, temp3, temp4, temp5);
        return new SelectProvider6<T1, TTemp1, TTemp2, TTemp3, TTemp4, TTemp5>(Executor, SqlBuilder);
    }
    #endregion

    #region TypeSet
    public IExpSelect<T1, TJoin> InnerJoin<TJoin>(Expression<Func<TypeSet<T1, TJoin>, bool>> exp)
    {
        var flatExp = global::LightORM.Utils.FlatTypeSet.Default.Flat(exp)!;
        this.JoinHandle<TJoin>(flatExp, ExpressionSql.TableLinkType.InnerJoin);
        return new SelectProvider2<T1, TJoin>(Executor, SqlBuilder);
    }
    public IExpSelect<T1, TJoin> LeftJoin<TJoin>(Expression<Func<TypeSet<T1, TJoin>, bool>> exp)
    {
        var flatExp = global::LightORM.Utils.FlatTypeSet.Default.Flat(exp)!;
        this.JoinHandle<TJoin>(flatExp, ExpressionSql.TableLinkType.LeftJoin);
        return new SelectProvider2<T1, TJoin>(Executor, SqlBuilder);
    }
    public IExpSelect<T1, TJoin> RightJoin<TJoin>(Expression<Func<TypeSet<T1, TJoin>, bool>> exp)
    {
        var flatExp = global::LightORM.Utils.FlatTypeSet.Default.Flat(exp)!;
        this.JoinHandle<TJoin>(flatExp, ExpressionSql.TableLinkType.RightJoin);
        return new SelectProvider2<T1, TJoin>(Executor, SqlBuilder);
    }
    public IExpSelect<T1, TJoin> InnerJoin<TJoin>(IExpSelect<TJoin> subQuery, Expression<Func<TypeSet<T1, TJoin>, bool>> exp)
    {
        var flatExp = global::LightORM.Utils.FlatTypeSet.Default.Flat(exp)!;
        this.JoinHandle<TJoin>(flatExp, ExpressionSql.TableLinkType.InnerJoin, subQuery);
        return new SelectProvider2<T1, TJoin>(Executor, SqlBuilder);
    }

    public IExpSelect<T1, TJoin> LeftJoin<TJoin>(IExpSelect<TJoin> subQuery, Expression<Func<TypeSet<T1, TJoin>, bool>> exp)
    {
        var flatExp = global::LightORM.Utils.FlatTypeSet.Default.Flat(exp)!;
        this.JoinHandle<TJoin>(flatExp, ExpressionSql.TableLinkType.LeftJoin, subQuery);
        return new SelectProvider2<T1, TJoin>(Executor, SqlBuilder);
    }

    public IExpSelect<T1, TJoin> RightJoin<TJoin>(IExpSelect<TJoin> subQuery, Expression<Func<TypeSet<T1, TJoin>, bool>> exp)
    {
        var flatExp = global::LightORM.Utils.FlatTypeSet.Default.Flat(exp)!;
        this.JoinHandle<TJoin>(flatExp, ExpressionSql.TableLinkType.RightJoin, subQuery);
        return new SelectProvider2<T1, TJoin>(Executor, SqlBuilder);
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
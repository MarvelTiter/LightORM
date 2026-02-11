using LightORM.Utils.Vistors;
using System.Threading;

namespace LightORM.Providers.Select;

internal sealed class SelectProvider2<T1, T2> : SelectProvider0<IExpSelect<T1, T2>, T1>, IExpSelect<T1, T2>
{
    public SelectProvider2(ISqlExecutor executor, SelectBuilder? builder = null)
        : base(executor, builder)
    {
        if (builder == null)
        {
            SqlBuilder = SelectBuilder.GetSelectBuilder();
            SqlBuilder.SelectedTables.Add(TableInfo.Create<T1>(0));
            SqlBuilder.SelectedTables.Add(TableInfo.Create<T2>(1));
        }
    }

    public IExpSelectGroup<TGroup, TypeSet<T1, T2>> GroupBy<TGroup>(Expression<Func<T1, T2, TGroup>> exp)
    {
        return this.GroupByHandle<TGroup, TypeSet<T1, T2>>(exp);
    }

    public IExpSelect<T1, T2> OrderBy<TOrder>(Expression<Func<T1, T2, TOrder>> exp)
    {
        this.OrderByHandle(exp, true);
        return this;
    }

    public IExpSelect<T1, T2> OrderByDesc<TOrder>(Expression<Func<T1, T2, TOrder>> exp)
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
        this.JoinHandle<TJoin>(exp, TableLinkType.InnerJoin);
        return new SelectProvider3<T1, T2, TJoin>(Executor, SqlBuilder);
    }

    public IExpSelect<T1, T2, TJoin> LeftJoin<TJoin>(Expression<Func<T1, T2, TJoin, bool>> exp)
    {
        this.JoinHandle<TJoin>(exp, TableLinkType.LeftJoin);
        return new SelectProvider3<T1, T2, TJoin>(Executor, SqlBuilder);
    }

    public IExpSelect<T1, T2, TJoin> RightJoin<TJoin>(Expression<Func<T1, T2, TJoin, bool>> exp)
    {
        this.JoinHandle<TJoin>(exp, TableLinkType.RightJoin);
        return new SelectProvider3<T1, T2, TJoin>(Executor, SqlBuilder);
    }

    public IExpSelect<T1, T2, TJoin> OuterJoin<TJoin>(Expression<Func<T1, T2, TJoin, bool>> exp)
    {
        this.JoinHandle<TJoin>(exp, TableLinkType.OuterJoin);
        return new SelectProvider3<T1, T2, TJoin>(Executor, SqlBuilder);
    }

    public IExpSelect<T1, T2, TJoin> InnerJoin<TJoin>(string tableName, Expression<Func<T1, T2, TJoin, bool>> exp)
    {
        this.JoinHandle<TJoin>(exp, TableLinkType.InnerJoin, overriddenTableName: tableName);
        return new SelectProvider3<T1, T2, TJoin>(Executor, SqlBuilder);
    }

    public IExpSelect<T1, T2, TJoin> LeftJoin<TJoin>(string tableName, Expression<Func<T1, T2, TJoin, bool>> exp)
    {
        this.JoinHandle<TJoin>(exp, TableLinkType.LeftJoin, overriddenTableName: tableName);
        return new SelectProvider3<T1, T2, TJoin>(Executor, SqlBuilder);
    }

    public IExpSelect<T1, T2, TJoin> RightJoin<TJoin>(string tableName, Expression<Func<T1, T2, TJoin, bool>> exp)
    {
        this.JoinHandle<TJoin>(exp, TableLinkType.RightJoin, overriddenTableName: tableName);
        return new SelectProvider3<T1, T2, TJoin>(Executor, SqlBuilder);
    }

    public IExpSelect<T1, T2, TJoin> OuterJoin<TJoin>(string tableName, Expression<Func<T1, T2, TJoin, bool>> exp)
    {
        this.JoinHandle<TJoin>(exp, TableLinkType.OuterJoin, overriddenTableName: tableName);
        return new SelectProvider3<T1, T2, TJoin>(Executor, SqlBuilder);
    }

    public IExpSelect<T1, T2, TJoin> InnerJoin<TJoin>(IExpSelect<TJoin> subQuery, Expression<Func<T1, T2, TJoin, bool>> where)
    {
        this.JoinHandle<TJoin>(where, TableLinkType.InnerJoin, subQuery);
        return new SelectProvider3<T1, T2, TJoin>(Executor, SqlBuilder);
    }

    public IExpSelect<T1, T2, TJoin> LeftJoin<TJoin>(IExpSelect<TJoin> subQuery, Expression<Func<T1, T2, TJoin, bool>> where)
    {
        this.JoinHandle<TJoin>(where, TableLinkType.LeftJoin, subQuery);
        return new SelectProvider3<T1, T2, TJoin>(Executor, SqlBuilder);
    }

    public IExpSelect<T1, T2, TJoin> RightJoin<TJoin>(IExpSelect<TJoin> subQuery, Expression<Func<T1, T2, TJoin, bool>> where)
    {
        this.JoinHandle<TJoin>(where, TableLinkType.RightJoin, subQuery);
        return new SelectProvider3<T1, T2, TJoin>(Executor, SqlBuilder);
    }

    public IExpSelect<T1, T2, TJoin> OuterJoin<TJoin>(IExpSelect<TJoin> subQuery, Expression<Func<T1, T2, TJoin, bool>> where)
    {
        this.JoinHandle<TJoin>(where, TableLinkType.OuterJoin, subQuery);
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

    public IExpSelect<T1, T2> OrderBy<TOrder>(Expression<Func<TypeSet<T1, T2>, TOrder>> exp)
    {
        var flatExp = FlatTypeSet.Default.Flat(exp)!;
        this.OrderByHandle(flatExp, true);
        return this;
    }

    public IExpSelect<T1, T2> OrderByDesc<TOrder>(Expression<Func<TypeSet<T1, T2>, TOrder>> exp)
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
        this.JoinHandle<TJoin>(flatExp, TableLinkType.InnerJoin);
        return new SelectProvider3<T1, T2, TJoin>(Executor, SqlBuilder);
    }

    public IExpSelect<T1, T2, TJoin> LeftJoin<TJoin>(Expression<Func<TypeSet<T1, T2, TJoin>, bool>> exp)
    {
        var flatExp = FlatTypeSet.Default.Flat(exp)!;
        this.JoinHandle<TJoin>(flatExp, TableLinkType.LeftJoin);
        return new SelectProvider3<T1, T2, TJoin>(Executor, SqlBuilder);
    }

    public IExpSelect<T1, T2, TJoin> RightJoin<TJoin>(Expression<Func<TypeSet<T1, T2, TJoin>, bool>> exp)
    {
        var flatExp = FlatTypeSet.Default.Flat(exp)!;
        this.JoinHandle<TJoin>(flatExp, TableLinkType.RightJoin);
        return new SelectProvider3<T1, T2, TJoin>(Executor, SqlBuilder);
    }

    public IExpSelect<T1, T2, TJoin> OuterJoin<TJoin>(Expression<Func<TypeSet<T1, T2, TJoin>, bool>> exp)
    {
        var flatExp = FlatTypeSet.Default.Flat(exp)!;
        this.JoinHandle<TJoin>(flatExp, TableLinkType.OuterJoin);
        return new SelectProvider3<T1, T2, TJoin>(Executor, SqlBuilder);
    }

    public IExpSelect<T1, T2, TJoin> InnerJoin<TJoin>(string tableName, Expression<Func<TypeSet<T1, T2, TJoin>, bool>> exp)
    {
        var flatExp = FlatTypeSet.Default.Flat(exp)!;
        this.JoinHandle<TJoin>(flatExp, TableLinkType.InnerJoin, overriddenTableName: tableName);
        return new SelectProvider3<T1, T2, TJoin>(Executor, SqlBuilder);
    }

    public IExpSelect<T1, T2, TJoin> LeftJoin<TJoin>(string tableName, Expression<Func<TypeSet<T1, T2, TJoin>, bool>> exp)
    {
        var flatExp = FlatTypeSet.Default.Flat(exp)!;
        this.JoinHandle<TJoin>(flatExp, TableLinkType.LeftJoin, overriddenTableName: tableName);
        return new SelectProvider3<T1, T2, TJoin>(Executor, SqlBuilder);
    }

    public IExpSelect<T1, T2, TJoin> RightJoin<TJoin>(string tableName, Expression<Func<TypeSet<T1, T2, TJoin>, bool>> exp)
    {
        var flatExp = FlatTypeSet.Default.Flat(exp)!;
        this.JoinHandle<TJoin>(flatExp, TableLinkType.RightJoin, overriddenTableName: tableName);
        return new SelectProvider3<T1, T2, TJoin>(Executor, SqlBuilder);
    }

    public IExpSelect<T1, T2, TJoin> OuterJoin<TJoin>(string tableName, Expression<Func<TypeSet<T1, T2, TJoin>, bool>> exp)
    {
        var flatExp = FlatTypeSet.Default.Flat(exp)!;
        this.JoinHandle<TJoin>(flatExp, TableLinkType.OuterJoin, overriddenTableName: tableName);
        return new SelectProvider3<T1, T2, TJoin>(Executor, SqlBuilder);
    }

    public IExpSelect<T1, T2, TJoin> InnerJoin<TJoin>(IExpSelect<TJoin> subQuery, Expression<Func<TypeSet<T1, T2, TJoin>, bool>> where)
    {
        var flatExp = FlatTypeSet.Default.Flat(where);
        this.JoinHandle<TJoin>(flatExp, TableLinkType.InnerJoin, subQuery);
        return new SelectProvider3<T1, T2, TJoin>(Executor, SqlBuilder);
    }

    public IExpSelect<T1, T2, TJoin> LeftJoin<TJoin>(IExpSelect<TJoin> subQuery, Expression<Func<TypeSet<T1, T2, TJoin>, bool>> where)
    {
        var flatExp = FlatTypeSet.Default.Flat(where);
        this.JoinHandle<TJoin>(flatExp, TableLinkType.LeftJoin, subQuery);
        return new SelectProvider3<T1, T2, TJoin>(Executor, SqlBuilder);
    }

    public IExpSelect<T1, T2, TJoin> RightJoin<TJoin>(IExpSelect<TJoin> subQuery, Expression<Func<TypeSet<T1, T2, TJoin>, bool>> where)
    {
        var flatExp = FlatTypeSet.Default.Flat(where);
        this.JoinHandle<TJoin>(flatExp, TableLinkType.RightJoin, subQuery);
        return new SelectProvider3<T1, T2, TJoin>(Executor, SqlBuilder);
    }

    public IExpSelect<T1, T2, TJoin> OuterJoin<TJoin>(IExpSelect<TJoin> subQuery, Expression<Func<TypeSet<T1, T2, TJoin>, bool>> where)
    {
        var flatExp = FlatTypeSet.Default.Flat(where);
        this.JoinHandle<TJoin>(flatExp, TableLinkType.OuterJoin, subQuery);
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
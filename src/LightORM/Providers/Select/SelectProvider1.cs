using LightORM.Extension;
using LightORM.Implements;
using LightORM.Utils.Vistors;
using System.Threading;

namespace LightORM.Providers.Select;

//TODO Select 匿名类
internal sealed class SelectProvider0(ISqlExecutor executor) : SelectProvider0<IExpSelect<object>, object>(executor), IExpSelect
{
}

internal class SelectProvider1<T1> : SelectProvider0<IExpSelect<T1>, T1>, IExpSelect<T1>
{
    public SelectProvider1(ISqlExecutor executor, SelectBuilder? builder = null)
        : base(executor, builder)
    {
        if (builder == null)
        {
            SqlBuilder = SelectBuilder.GetSelectBuilder();
            //SqlBuilder.SelectedTables.Add(TableContext.GetTableInfo<T1>());
            SqlBuilder.SelectedTables.Add(TableInfo.Create<T1>());
        }
    }

    public SelectProvider1(string overriddenTableName, ISqlExecutor executor)
        : base(executor)
    {
        SqlBuilder = SelectBuilder.GetSelectBuilder();
        SqlBuilder.SelectedTables.Add(TableInfo.Create<T1>(overriddenTableName));
    }

    public IExpSelect<T1> As(string alias)
    {
        throw new NotImplementedException("暂不支持自定义alias");
    }

    public IExpSelect<T1> Union(IExpSelect<T1> select)
    {
        //var provider = this.HandleSubQuery<T1>();
        SqlBuilder.AddUnion(select.SqlBuilder, false);
        return this;
    }

    public IExpSelect<T1> UnionAll(IExpSelect<T1> select)
    {
        //var provider = this.HandleSubQuery<T1>();
        SqlBuilder.AddUnion(select.SqlBuilder, true);
        return this;
    }

    public IExpSelectGroup<TGroup, T1> GroupBy<TGroup>(Expression<Func<T1, TGroup>> exp)
    {
        return this.GroupByHandle<TGroup, T1>(exp);
    }

    public IExpSelect<T1> OrderBy<TOrder>(Expression<Func<T1, TOrder>> exp)
    {
        this.OrderByHandle(exp, true);
        return this;
    }

    public IExpSelect<T1> OrderByDesc<TOrder>(Expression<Func<T1, TOrder>> exp)
    {
        this.OrderByHandle(exp, false);
        return this;
    }

    #region Join

    public IExpSelect<T1, TJoin> Join<TJoin>(TableLinkType joinType, Expression<Func<T1, TJoin, bool>> exp)
    {
        this.JoinHandle<TJoin>(exp, joinType);
        return new SelectProvider2<T1, TJoin>(Executor, SqlBuilder);
    }

    public IExpSelect<T1, TJoin> InnerJoin<TJoin>(Expression<Func<T1, TJoin, bool>> exp)
    {
        this.JoinHandle<TJoin>(exp, TableLinkType.InnerJoin);
        return new SelectProvider2<T1, TJoin>(Executor, SqlBuilder);
    }

    public IExpSelect<T1, TJoin> LeftJoin<TJoin>(Expression<Func<T1, TJoin, bool>> exp)
    {
        this.JoinHandle<TJoin>(exp, TableLinkType.LeftJoin);
        return new SelectProvider2<T1, TJoin>(Executor, SqlBuilder);
    }

    public IExpSelect<T1, TJoin> RightJoin<TJoin>(Expression<Func<T1, TJoin, bool>> exp)
    {
        this.JoinHandle<TJoin>(exp, TableLinkType.RightJoin);
        return new SelectProvider2<T1, TJoin>(Executor, SqlBuilder);
    }

    public IExpSelect<T1, TJoin> OuterJoin<TJoin>(Expression<Func<T1, TJoin, bool>> exp)
    {
        this.JoinHandle<TJoin>(exp, TableLinkType.OuterJoin);
        return new SelectProvider2<T1, TJoin>(Executor, SqlBuilder);
    }

    public IExpSelect<T1, TJoin> InnerJoin<TJoin>(string tableName, Expression<Func<T1, TJoin, bool>> exp)
    {
        this.JoinHandle<TJoin>(exp, TableLinkType.InnerJoin, overriddenTableName: tableName);
        return new SelectProvider2<T1, TJoin>(Executor, SqlBuilder);
    }

    public IExpSelect<T1, TJoin> LeftJoin<TJoin>(string tableName, Expression<Func<T1, TJoin, bool>> exp)
    {
        this.JoinHandle<TJoin>(exp, TableLinkType.LeftJoin, overriddenTableName: tableName);
        return new SelectProvider2<T1, TJoin>(Executor, SqlBuilder);
    }

    public IExpSelect<T1, TJoin> RightJoin<TJoin>(string tableName, Expression<Func<T1, TJoin, bool>> exp)
    {
        this.JoinHandle<TJoin>(exp, TableLinkType.RightJoin, overriddenTableName: tableName);
        return new SelectProvider2<T1, TJoin>(Executor, SqlBuilder);
    }

    public IExpSelect<T1, TJoin> OuterJoin<TJoin>(string tableName, Expression<Func<T1, TJoin, bool>> exp)
    {
        this.JoinHandle<TJoin>(exp, TableLinkType.OuterJoin, overriddenTableName: tableName);
        return new SelectProvider2<T1, TJoin>(Executor, SqlBuilder);
    }

    public IExpSelect<T1, TJoin> InnerJoin<TJoin>(IExpSelect<TJoin> subQuery, Expression<Func<T1, TJoin, bool>> exp)
    {
        this.JoinHandle<TJoin>(exp, TableLinkType.InnerJoin, subQuery);
        return new SelectProvider2<T1, TJoin>(Executor, SqlBuilder);
    }

    public IExpSelect<T1, TJoin> LeftJoin<TJoin>(IExpSelect<TJoin> subQuery, Expression<Func<T1, TJoin, bool>> exp)
    {
        this.JoinHandle<TJoin>(exp, TableLinkType.LeftJoin, subQuery);
        return new SelectProvider2<T1, TJoin>(Executor, SqlBuilder);
    }

    public IExpSelect<T1, TJoin> RightJoin<TJoin>(IExpSelect<TJoin> subQuery, Expression<Func<T1, TJoin, bool>> exp)
    {
        this.JoinHandle<TJoin>(exp, TableLinkType.RightJoin, subQuery);
        return new SelectProvider2<T1, TJoin>(Executor, SqlBuilder);
    }

    public IExpSelect<T1, TJoin> OuterJoin<TJoin>(IExpSelect<TJoin> subQuery, Expression<Func<T1, TJoin, bool>> exp)
    {
        this.JoinHandle<TJoin>(exp, TableLinkType.OuterJoin, subQuery);
        return new SelectProvider2<T1, TJoin>(Executor, SqlBuilder);
    }

    public IExpSelect<T1, TJoin> InnerJoin<TJoin>(IExpTemp<TJoin> subQuery, Expression<Func<T1, TJoin, bool>> where)
    {
        this.JoinHandle<TJoin>(where, TableLinkType.InnerJoin, subQuery);
        return new SelectProvider2<T1, TJoin>(Executor, SqlBuilder);
    }

    public IExpSelect<T1, TJoin> LeftJoin<TJoin>(IExpTemp<TJoin> subQuery, Expression<Func<T1, TJoin, bool>> where)
    {
        this.JoinHandle<TJoin>(where, TableLinkType.LeftJoin, subQuery);
        return new SelectProvider2<T1, TJoin>(Executor, SqlBuilder);
    }

    public IExpSelect<T1, TJoin> RightJoin<TJoin>(IExpTemp<TJoin> subQuery, Expression<Func<T1, TJoin, bool>> where)
    {
        this.JoinHandle<TJoin>(where, TableLinkType.RightJoin, subQuery);
        return new SelectProvider2<T1, TJoin>(Executor, SqlBuilder);
    }

    public IExpSelect<T1, TJoin> OuterJoin<TJoin>(IExpTemp<TJoin> subQuery, Expression<Func<T1, TJoin, bool>> where)
    {
        this.JoinHandle<TJoin>(where, TableLinkType.OuterJoin, subQuery);
        return new SelectProvider2<T1, TJoin>(Executor, SqlBuilder);
    }

    #endregion

    public IExpInclude<T1, TMember> Include<TMember>(Expression<Func<T1, TMember>> exp)
    {
        return CreateIncludeProvider<TMember>(exp.Body);
    }

    public string ToSql(Expression<Func<T1, object>> exp)
    {
        this.HandleResult(exp, null);
        return ToSql();
    }

    internal IExpInclude<T1, TMember> CreateIncludeProvider<TMember>(Expression exp)
    {
        //var option = SqlResolveOptions.Select;
        //var result = exp.Resolve(option, ResolveContext.Create(Executor.Database.DbBaseType));
        string? includePropertyName = null;
        Expression? includeWhereExpression = null;
        if (exp is MemberExpression m)
        {
            includePropertyName = m.Member.Name;
        }
        else if (exp is MethodCallExpression mc)
        {
            var member = mc.Arguments[0] as MemberExpression;
            includePropertyName = member?.Member.Name;
            if (mc.Arguments.Count > 1)
            {
                includeWhereExpression = mc.Arguments[1];
            }
        }
        if (string.IsNullOrEmpty(includePropertyName))
        {
            throw new LightOrmException("解析导航属性失败");
        }
        var navCol = SqlBuilder.MainTable.GetColumnInfo(includePropertyName!);
        var navInfo = navCol.NavigateInfo!;
        var parentWhereColumn = SqlBuilder.MainTable.GetColumnInfo(navCol.NavigateInfo!.MainName!);
        var includeInfo = new IncludeInfo
        {
            NavigateInfo = navInfo,
            ParentNavigateColumn = navCol,
            ParentWhereColumn = parentWhereColumn,
            ParentTable = SqlBuilder.MainTable,
            IncludeWhereExpression = includeWhereExpression
        };
        SqlBuilder.IncludeContext.Includes.Add(includeInfo);

        return new IncludeProvider<T1, TMember>(Executor, SqlBuilder);
    }

    public IExpSelect<T1> AsSubQuery(string? alias = null)
    {
        //this.HandleResult(exp, null);
        var sub = this.HandleSubQuery<T1>(alias);
        return sub;
    }

    public IExpTemp<TTemp> AsTemp<TTemp>(string name, Expression<Func<T1, TTemp>> exp)
    {
        this.HandleResult(exp, null);
        return new TempProvider<TTemp>(name, SqlBuilder);
    }

    public IExpSelect<TTable> AsTable<TTable>(Expression<Func<T1, TTable>> exp)
    {
        this.HandleResult(exp, null);
        return new SelectProvider1<TTable>(Executor, SqlBuilder);
    }

    #region Result

    public IEnumerable<TReturn> ToList<TReturn>(Expression<Func<T1, TReturn>> exp)
    {
        this.HandleResult(exp, null);
        return this.InternalToList<TReturn>();
    }

    public Task<IList<TReturn>> ToListAsync<TReturn>(Expression<Func<T1, TReturn>> exp, CancellationToken cancellationToken = default)
    {
        this.HandleResult(exp, null);
        return this.InternalToListAsync<TReturn>(cancellationToken);
    }

    public IEnumerable<TReturn> ToList<TReturn>(Expression<Func<T1, object>> exp)
    {
        this.HandleResult(exp, null);
        return this.InternalToList<TReturn>();
    }

    public Task<IList<TReturn>> ToListAsync<TReturn>(Expression<Func<T1, object>> exp, CancellationToken cancellationToken = default)
    {
        this.HandleResult(exp, null);
        return this.InternalToListAsync<TReturn>(cancellationToken);
    }

    public IEnumerable<TReturn> ToList<TReturn>() => this.InternalToList<TReturn>();

    public Task<IList<TReturn>> ToListAsync<TReturn>(CancellationToken cancellationToken = default) => this.InternalToListAsync<TReturn>(cancellationToken);

    public IAsyncEnumerable<TReturn> ToEnumerableAsync<TReturn>(Expression<Func<T1, TReturn>> exp, CancellationToken cancellationToken = default)
    {
        this.HandleResult(exp, null);
        return this.InternalToEnumerableAsync<TReturn>(cancellationToken);
    }

    public IAsyncEnumerable<TReturn> ToEnumerableAsync<TReturn>(Expression<Func<T1, object>> exp, CancellationToken cancellationToken = default)
    {
        this.HandleResult(exp, null);
        return this.InternalToEnumerableAsync<TReturn>(cancellationToken);
    }

    public IAsyncEnumerable<TReturn> ToEnumerableAsync<TReturn>(CancellationToken cancellationToken = default)
        => this.InternalToEnumerableAsync<TReturn>(cancellationToken);

    #endregion

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
        var flatExp = FlatTypeSet.Default.Flat(exp)!;
        this.JoinHandle<TJoin>(flatExp, TableLinkType.InnerJoin);
        return new SelectProvider2<T1, TJoin>(Executor, SqlBuilder);
    }

    public IExpSelect<T1, TJoin> LeftJoin<TJoin>(Expression<Func<TypeSet<T1, TJoin>, bool>> exp)
    {
        var flatExp = FlatTypeSet.Default.Flat(exp)!;
        this.JoinHandle<TJoin>(flatExp, TableLinkType.LeftJoin);
        return new SelectProvider2<T1, TJoin>(Executor, SqlBuilder);
    }

    public IExpSelect<T1, TJoin> RightJoin<TJoin>(Expression<Func<TypeSet<T1, TJoin>, bool>> exp)
    {
        var flatExp = FlatTypeSet.Default.Flat(exp)!;
        this.JoinHandle<TJoin>(flatExp, TableLinkType.RightJoin);
        return new SelectProvider2<T1, TJoin>(Executor, SqlBuilder);
    }

    public IExpSelect<T1, TJoin> OuterJoin<TJoin>(Expression<Func<TypeSet<T1, TJoin>, bool>> exp)
    {
        var flatExp = FlatTypeSet.Default.Flat(exp)!;
        this.JoinHandle<TJoin>(flatExp, TableLinkType.OuterJoin);
        return new SelectProvider2<T1, TJoin>(Executor, SqlBuilder);
    }

    public IExpSelect<T1, TJoin> InnerJoin<TJoin>(string tableName, Expression<Func<TypeSet<T1, TJoin>, bool>> exp)
    {
        var flatExp = FlatTypeSet.Default.Flat(exp)!;
        this.JoinHandle<TJoin>(flatExp, TableLinkType.InnerJoin, overriddenTableName: tableName);
        return new SelectProvider2<T1, TJoin>(Executor, SqlBuilder);
    }

    public IExpSelect<T1, TJoin> LeftJoin<TJoin>(string tableName, Expression<Func<TypeSet<T1, TJoin>, bool>> exp)
    {
        var flatExp = FlatTypeSet.Default.Flat(exp)!;
        this.JoinHandle<TJoin>(flatExp, TableLinkType.LeftJoin, overriddenTableName: tableName);
        return new SelectProvider2<T1, TJoin>(Executor, SqlBuilder);
    }

    public IExpSelect<T1, TJoin> RightJoin<TJoin>(string tableName, Expression<Func<TypeSet<T1, TJoin>, bool>> exp)
    {
        var flatExp = FlatTypeSet.Default.Flat(exp)!;
        this.JoinHandle<TJoin>(flatExp, TableLinkType.RightJoin, overriddenTableName: tableName);
        return new SelectProvider2<T1, TJoin>(Executor, SqlBuilder);
    }

    public IExpSelect<T1, TJoin> OuterJoin<TJoin>(string tableName, Expression<Func<TypeSet<T1, TJoin>, bool>> exp)
    {
        var flatExp = FlatTypeSet.Default.Flat(exp)!;
        this.JoinHandle<TJoin>(flatExp, TableLinkType.OuterJoin, overriddenTableName: tableName);
        return new SelectProvider2<T1, TJoin>(Executor, SqlBuilder);
    }

    public IExpSelect<T1, TJoin> InnerJoin<TJoin>(IExpSelect<TJoin> subQuery, Expression<Func<TypeSet<T1, TJoin>, bool>> exp)
    {
        var flatExp = FlatTypeSet.Default.Flat(exp)!;
        this.JoinHandle<TJoin>(flatExp, TableLinkType.InnerJoin, subQuery);
        return new SelectProvider2<T1, TJoin>(Executor, SqlBuilder);
    }

    public IExpSelect<T1, TJoin> LeftJoin<TJoin>(IExpSelect<TJoin> subQuery, Expression<Func<TypeSet<T1, TJoin>, bool>> exp)
    {
        var flatExp = FlatTypeSet.Default.Flat(exp)!;
        this.JoinHandle<TJoin>(flatExp, TableLinkType.LeftJoin, subQuery);
        return new SelectProvider2<T1, TJoin>(Executor, SqlBuilder);
    }

    public IExpSelect<T1, TJoin> RightJoin<TJoin>(IExpSelect<TJoin> subQuery, Expression<Func<TypeSet<T1, TJoin>, bool>> exp)
    {
        var flatExp = FlatTypeSet.Default.Flat(exp)!;
        this.JoinHandle<TJoin>(flatExp, TableLinkType.RightJoin, subQuery);
        return new SelectProvider2<T1, TJoin>(Executor, SqlBuilder);
    }

    public IExpSelect<T1, TJoin> OuterJoin<TJoin>(IExpSelect<TJoin> subQuery, Expression<Func<TypeSet<T1, TJoin>, bool>> exp)
    {
        var flatExp = FlatTypeSet.Default.Flat(exp)!;
        this.JoinHandle<TJoin>(flatExp, TableLinkType.OuterJoin, subQuery);
        return new SelectProvider2<T1, TJoin>(Executor, SqlBuilder);
    }

    #endregion

    #region Insert

    public ISelectInsert<TInsertTable> Insert<TInsertTable>()
    {
        return Insert<TInsertTable>(t => t!);
    }

    //public Task<int> InsertAsync<TInsertTable>(CancellationToken cancellationToken = default)
    //{
    //    return InsertAsync<TInsertTable>(t => t!, cancellationToken);
    //}

    public ISelectInsert<TInsertTable> Insert<TInsertTable>(Expression<Func<TInsertTable, object>> exp)
    {
        var table = TableContext.GetTableInfo<TInsertTable>();
        var result = exp.Resolve(SqlResolveOptions.Insert, ResolveContext.Create(Executor.Database.DbBaseType, table));
        HandleSelectInsert(table.TableName, result.SqlString!);
        //var sql = SqlBuilder.ToSqlString();
        SqlBuilder.Expressions.Update(e =>
        {
            if (e.ResolveOptions == SqlResolveOptions.Select)
            {
                e.ResolveOptions = SqlResolveOptions.Select with { UseColumnAlias = false };
            }
        });
        return new SelectInsertProvider<TInsertTable>(Executor, SqlBuilder);
        //return Executor.ExecuteNonQuery(sql, SqlBuilder.DbParameters);
    }

    //public Task<int> InsertAsync<TInsertTable>(Expression<Func<TInsertTable, object>> exp, CancellationToken cancellationToken = default)
    //{
    //    var table = TableContext.GetTableInfo<TInsertTable>();
    //    var result = exp.Resolve(SqlResolveOptions.Insert, ResolveContext.Create(Executor.Database.DbBaseType, table));
    //    HandleSelectInsert(table.TableName, result.SqlString!);
    //    var sql = SqlBuilder.ToSqlString();
    //    return Executor.ExecuteNonQueryAsync(sql, SqlBuilder.DbParameters, cancellationToken: cancellationToken);
    //}

    public ISelectInsert<object> Insert(string tableName, params string[] columns)
    {
        HandleSelectInsert(tableName, string.Join(", ", columns));
        //var sql = SqlBuilder.ToSqlString();
        return new SelectInsertProvider<object>(Executor, SqlBuilder);
        //return Executor.ExecuteNonQuery(sql, SqlBuilder.DbParameters);
    }

    //public Task<int> InsertAsync(string tableName, string[] columns, CancellationToken cancellationToken = default)
    //{
    //    HandleSelectInsert(tableName, string.Join(", ", columns));
    //    var sql = SqlBuilder.ToSqlString();
    //    return Executor.ExecuteNonQueryAsync(sql, SqlBuilder.DbParameters, cancellationToken: cancellationToken);
    //}

    void HandleSelectInsert(string tableName, string columns)
    {
        SqlBuilder.InsertInfo = new(tableName, columns);
    }

    #endregion
}
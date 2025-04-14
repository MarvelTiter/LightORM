using LightORM.Extension;
using LightORM.Interfaces.ExpSql;
using System.Text;
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
            SqlBuilder = new SelectBuilder(DbType);
            //SqlBuilder.SelectedTables.Add(TableContext.GetTableInfo<T1>());
            SqlBuilder.SelectedTables.Add(TableInfo.Create<T1>());
        }
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

    public IExpSelect<T1, TJoin> InnerJoin<TJoin>(IExpTemp<TJoin> subQuery, Expression<Func<T1, TJoin, bool>> where)
    {
        this.JoinHandle<TJoin>(where, ExpressionSql.TableLinkType.InnerJoin, subQuery);
        return new SelectProvider2<T1, TJoin>(Executor, SqlBuilder);
    }

    public IExpSelect<T1, TJoin> LeftJoin<TJoin>(IExpTemp<TJoin> subQuery, Expression<Func<T1, TJoin, bool>> where)
    {
        this.JoinHandle<TJoin>(where, ExpressionSql.TableLinkType.LeftJoin, subQuery);
        return new SelectProvider2<T1, TJoin>(Executor, SqlBuilder);
    }
    public IExpSelect<T1, TJoin> RightJoin<TJoin>(IExpTemp<TJoin> subQuery, Expression<Func<T1, TJoin, bool>> where)
    {
        this.JoinHandle<TJoin>(where, ExpressionSql.TableLinkType.RightJoin, subQuery);
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
        LightOrmException.ThrowIfNull(includePropertyName, "解析导航属性失败");
        // TODO 修复Include
        var navCol = SqlBuilder.MainTable.GetColumnInfo(includePropertyName!);
        var navInfo = navCol.NavigateInfo!;
        var table = TableInfo.Create(navCol.NavigateInfo!.NavigateType);
        var parentWhereColumn = SqlBuilder.MainTable.GetColumnInfo(navCol.NavigateInfo!.MainName!);
        var includeInfo = new IncludeInfo
        {
            SelectedTable = table,
            NavigateInfo = navInfo,
            ParentNavigateColumn = navCol,
            ParentWhereColumn = parentWhereColumn,
            ParentTable = SqlBuilder.MainTable,
            //ExpressionResolvedResult = result
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

    public IExpSelect<TTable> AsTable<TTable>(Expression<Func<T1, TTable>> exp, string? alias = null)
    {
        this.HandleResult(exp, null);
        return new SelectProvider1<TTable>(Executor, SqlBuilder).AsSubQuery(alias);
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

    #region Insert

    public int Insert<TInsertTable>()
    {
        return Insert<TInsertTable>(t => t!);
    }

    public Task<int> InsertAsync<TInsertTable>(CancellationToken cancellationToken = default)
    {
        return InsertAsync<TInsertTable>(t => t!, cancellationToken);
    }

    public int Insert<TInsertTable>(Expression<Func<TInsertTable, object>> exp)
    {
        var table = TableContext.GetTableInfo<TInsertTable>();
        var result = exp.Resolve(SqlResolveOptions.Insert, ResolveContext.Create(Executor.Database.DbBaseType, table));
        HandleSelectInsert(table.TableName, result.SqlString!);
        var sql = SqlBuilder.ToSqlString();
        return Executor.ExecuteNonQuery(sql, SqlBuilder.DbParameters);
    }

    public Task<int> InsertAsync<TInsertTable>(Expression<Func<TInsertTable, object>> exp, CancellationToken cancellationToken = default)
    {
        var table = TableContext.GetTableInfo<TInsertTable>();
        var result = exp.Resolve(SqlResolveOptions.Insert, ResolveContext.Create(Executor.Database.DbBaseType, table));
        HandleSelectInsert(table.TableName, result.SqlString!);
        var sql = SqlBuilder.ToSqlString();
        return Executor.ExecuteNonQueryAsync(sql, SqlBuilder.DbParameters, cancellationToken: cancellationToken);
    }

    public int Insert(string tableName, params string[] columns)
    {
        HandleSelectInsert(tableName, string.Join(", ", columns));
        var sql = SqlBuilder.ToSqlString();
        return Executor.ExecuteNonQuery(sql, SqlBuilder.DbParameters);
    }

    public Task<int> InsertAsync(string tableName, string[] columns, CancellationToken cancellationToken = default)
    {
        HandleSelectInsert(tableName, string.Join(", ", columns));
        var sql = SqlBuilder.ToSqlString();
        return Executor.ExecuteNonQueryAsync(sql, SqlBuilder.DbParameters, cancellationToken: cancellationToken);
    }

    void HandleSelectInsert(string tableName, string columns)
    {
        SqlBuilder.InsertInfo = new(tableName, columns);
    }

    #endregion

}
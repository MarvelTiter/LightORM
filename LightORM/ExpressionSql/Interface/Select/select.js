let genericType = function (count) {
    let types = [];
    for (let index = 0; index < count; index++) {
        types.push(`T${index + 1}`);
    }
    return types.join();
};

let genericInit = function (count) {
    let types = [];
    for (let index = 1; index < count; index++) {
        types.push(`context.AddTable(typeof(T${index + 1}));`);
    }
    return types.join("\n");
};

function GenerateProvider(count) {
    let template = `
internal partial class SelectProvider${count}<${genericType(count)}> : BasicSelect0<IExpSelect<${genericType(count)}>, T1>, IExpSelect<${genericType(count)}>
{
    public SelectProvider${count}(string key, Func<string, ITableContext> getContext, DbConnectInfo connectInfos, SqlExecuteLife life)
     : base(key, getContext, connectInfos, life)
    {
        ${genericInit(count)}
    }

    public IExpSelect<${genericType(count)}> InnerJoin<TAnother>(Expression<Func<TypeSet<TAnother, ${genericType(count)}>, bool>> exp)
    {
        JoinHandle<TAnother>(TableLinkType.InnerJoin, exp.Body);
        return this;
    }

    public IExpSelect<${genericType(count)}> LeftJoin<TAnother>(Expression<Func<TypeSet<TAnother, ${genericType(count)}>, bool>> exp)
    {
        JoinHandle<TAnother>(TableLinkType.LeftJoin, exp.Body);
        return this;
    }
    public IExpSelect<${genericType(count)}> RightJoin<TAnother>(Expression<Func<TypeSet<TAnother, ${genericType(count)}>, bool>> exp)
    {
        JoinHandle<TAnother>(TableLinkType.RightJoin, exp.Body);
        return this;
    }

    public IExpSelect<${genericType(count)}> GroupBy(Expression<Func<TypeSet<${genericType(count)}>, object>> exp)
    {
        GroupByHandle(exp.Body);
        return this;
    }

    public IExpSelect<${genericType(count)}> OrderBy(Expression<Func<TypeSet<${genericType(count)}>, object>> exp, bool asc = true)
    {
        OrderByHandle(exp.Body, asc);
        return this;
    }

    public IEnumerable<TReturn> ToList<TReturn>(Expression<Func<TypeSet<${genericType(count)}>, object>> exp)
    {
        SelectHandle(exp.Body);
        return InternalQuery<TReturn>();
    }   

    public Task<IList<TReturn>> ToListAsync<TReturn>(Expression<Func<TypeSet<${genericType(count)}>, object>> exp)
    {
        SelectHandle(exp.Body);
        return InternalQueryAsync<TReturn>();
    }    

    public IExpSelect<${genericType(count)}> Where(Expression<Func<TypeSet<${genericType(count)}>, bool>> exp)
    {
        WhereHandle(exp.Body);
        return this;
    }
}
`;
    output.innerText += template
}

function Generate(count) {//SelectExtension
    let template = `
public static IExpSelect<${genericType(count)}> Select<${genericType(count)}>(this IExpSql self, string key = "MainDb") where T1 : class, new()
{
    var ins = self as ExpressionCoreSql;
    return new SelectProvider${count}<${genericType(count)}>(key, ins!.GetContext, ins!.GetDbInfo(key), ins!.Life);
}
`;
    output.innerText += template
}

function GenerateInterface(count) {
    let template = `
public interface IExpSelect<${genericType(count)}> : IExpSelect0<IExpSelect<${genericType(count)}>, T1>
{
     IEnumerable<TReturn> ToList<TReturn>(Expression<Func<TypeSet<${genericType(count)}>, object>> exp);
     Task<IList<TReturn>> ToListAsync<TReturn>(Expression<Func<TypeSet<${genericType(count)}>, object>> exp);
     IExpSelect<${genericType(count)}> Where(Expression<Func<TypeSet<${genericType(count)}>, bool>> exp);
     IExpSelect<${genericType(count)}> OrderBy(Expression<Func<TypeSet<${genericType(count)}>, object>> exp, bool asc = true);
     IExpSelect<${genericType(count)}> GroupBy(Expression<Func<TypeSet<${genericType(count)}>, object>> exp);
     IExpSelect<${genericType(count)}> InnerJoin<TAnother>(Expression<Func<TypeSet<TAnother,${genericType(count)}>, bool>> exp);
     IExpSelect<${genericType(count)}> LeftJoin<TAnother>(Expression<Func<TypeSet<TAnother,${genericType(count)}>, bool>> exp);
     IExpSelect<${genericType(count)}> RightJoin<TAnother>(Expression<Func<TypeSet<TAnother,${genericType(count)}>, bool>> exp);
}
`;
    output.innerText += template
}

function GenerateDynamicExt(count) {
    let template = `
#region T\`${count}
public static IEnumerable<dynamic> ToDynamicList<${genericType(count)}>(this IExpSelect<${genericType(count)}> self, Expression<Func<${genericType(count)}, object>> exp)
{
    var ins = self as SelectProvider${count}<${genericType(count)}>;
    ins!.SelectHandle(exp.Body);
    return ins!.InternalQuery();
}
public static Task<IList<dynamic>> ToDynamicListAsync<${genericType(count)}>(this IExpSelect<${genericType(count)}> self, Expression<Func<${genericType(count)}, object>> exp)
{
    var ins = self as SelectProvider${count}<${genericType(count)}>;
    ins!.SelectHandle(exp.Body);
    return ins!.InternalQueryAsync();
}
public static IEnumerable<dynamic> ToDynamicList<${genericType(count)}>(this IExpSelect<${genericType(count)}> self, Expression<Func<TypeSet<${genericType(count)}>, object>> exp)
{
    var ins = self as SelectProvider${count}<${genericType(count)}>;
    ins!.SelectHandle(exp.Body);
    return ins!.InternalQuery();
}
public static Task<IList<dynamic>> ToDynamicListAsync<${genericType(count)}>(this IExpSelect<${genericType(count)}> self, Expression<Func<TypeSet<${genericType(count)}>, object>> exp)
{
    var ins = self as SelectProvider${count}<${genericType(count)}>;
    ins!.SelectHandle(exp.Body);
    return ins!.InternalQueryAsync();
}
#endregion
`;
    output.innerText += template
}
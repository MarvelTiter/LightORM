let genericType = function (count) {
    let types = [];
    for (let index = 0; index < count; index++) {
        types.push(`T${index + 1}`);
    }
    return types.join();
};

let genericParam = function (count) {
    let types = [];
    for (let index = 0; index < count; index++) {
        types.push(`t${index + 1}`);
    }
    return types.join();
};

let genericInit = function (count) {
    let types = [];
    for (let index = 1; index < count; index++) {
        types.push(`SelectExpression = new ExpressionInfo()
        {
            Expression = exp,
            ResolveOptions = SqlResolveOptions.Select
        };
        SqlBuilder.Expressions.Add(SelectExpression);`);
    }
    return types.join("\n");
};

function GenerateProvider(count) {
    let template = `
internal partial class SelectProvider${count}<${genericType(count)}> : SelectProvider0<IExpSelect<${genericType(count)}>, T1>, IExpSelect<${genericType(count)}>
{
    public SelectProvider${count}(Expression exp, ISqlExecutor executor): base(executor)
    {
        SelectExpression = new ExpressionInfo()
        {
            Expression = exp,
            ResolveOptions = SqlResolveOptions.Select
        };
        SqlBuilder.Expressions.Add(SelectExpression);
    }

    public IExpSelect<${genericType(count)}> InnerJoin<TJoin>(Expression<Func<TypeSet<${genericType(count)}>, bool>> exp)
    {
        return JoinHandle<TAnother>(exp, ExpressionSql.TableLinkType.InnerJoin);
    }

    public IExpSelect<${genericType(count)}> LeftJoin<TJoin>(Expression<Func<TypeSet<${genericType(count)}>, bool>> exp)
    {
        return JoinHandle<TAnother>(exp, ExpressionSql.TableLinkType.LeftJoin);
    }
    public IExpSelect<${genericType(count)}> RightJoin<TJoin>(Expression<Func<TypeSet<${genericType(count)}>, bool>> exp)
    {
        return JoinHandle<TAnother>(exp, ExpressionSql.TableLinkType.RightJoin);
    }

    public IExpSelect<${genericType(count)}> GroupBy(Expression<Func<TypeSet<${genericType(count)}>, object>> exp)
    {
        return GroupByHandle(exp);
    }

    public IExpSelect<${genericType(count)}> OrderBy(Expression<Func<TypeSet<${genericType(count)}>, object>> exp, bool asc = true)
    {
        return OrderByHandle(exp, asc);
    }
        
    public IExpSelect<${genericType(count)}> Where(Expression<Func<TypeSet<${genericType(count)}>, bool>> exp)
    {
        return WhereHandle(exp);
    }
}
`;
    output.innerText += template
}

function Generate(count) {//SelectExtension
    let template = `
#region ${count}个类型参数
public static IExpSelect<${genericType(count)}> Select<${genericType(count)}>(this IExpressionContext self) where T1 : class, new()
{
    return Select<${genericType(count)}>(self, (${genericParam(count)}) => new { ${genericParam(count)} });
}
public static IExpSelect<${genericType(count)}> Select<${genericType(count)}>(this IExpressionContext self, Expression<Func<${genericType(count)}, object>> exp) where T1 : class, new()
{
    var ins = self as ExpressionCoreSql;
    return CreateProvider<${genericType(count)}>(ins!.CurrentKey, ins, exp.Body);
}
public static IExpSelect<${genericType(count)}> Select<${genericType(count)}>(this IExpressionContext self, Expression<Func<TypeSet<${genericType(count)}>, object>> exp) where T1 : class, new()
{
    var ins = self as ExpressionCoreSql;
    return CreateProvider<${genericType(count)}>(ins!.CurrentKey, ins, exp.Body);
}
static IExpSelect<${genericType(count)}> CreateProvider<${genericType(count)}>(string key, ExpressionCoreSql core, Expression body) => new SelectProvider${count}<${genericType(count)}>(body, core.GetContext(key), core.GetDbInfo(key), core.Life);
#endregion
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
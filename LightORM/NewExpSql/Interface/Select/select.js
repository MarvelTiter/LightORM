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

function Generate2(count) {
    let template = `
internal partial class SelectProvider${count}<${genericType(count)}> : BasicSelect0<IExpSelect<${genericType(count)}>, T1>, IExpSelect<${genericType(count)}>
{
    public SelectProvider${count}(string key, Func<string, ITableContext> getContext, DbConnectInfo connectInfos)
     : base(key, getContext, connectInfos)
    {
        ${genericInit(count)}
    }
    public IEnumerable<TReturn> ToList<TReturn>(Expression<Func<${genericType(count)}, TReturn>> exp)
    {
        SelectHandle(exp.Body);
        return InternalQuery<TReturn>(ToSql(), context.GetParameters());
    }

    public IEnumerable<TReturn> ToList<TReturn>(Expression<Func<TypeSet<${genericType(count)}>, TReturn>> exp)
    {
        SelectHandle(exp.Body);
        return InternalQuery<TReturn>(ToSql(), context.GetParameters());
    }

    public IExpSelect<${genericType(count)}> Where(Expression<Func<${genericType(count)}, bool>> exp)
    {
        WhereHandle(exp.Body);
        return this;
    }

    public IExpSelect<${genericType(count)}> Where(Expression<Func<TypeSet<${genericType(count)}>, bool>> exp)
    {
        WhereHandle(exp.Body);
        return this;
    }
}
`;
    output.innerText = template
}

function Generate(count) {
    let template = `
public static IExpSelect<${genericType(count)}> Select<${genericType(count)}>(this IExpSql self, string key = "MainDb") where T1 : class, new()
{
    var ins = self as ExpressionSql;
    return new SelectProvider${count}<${genericType(count)}>(key, ins.GetContext, ins.GetDbInfo(key));
}
`;
    output.innerText += template
}
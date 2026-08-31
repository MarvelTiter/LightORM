using LightORM.Extension;

namespace LightORM;

public class ResolveContext
{
    public readonly record struct ParameterKey(Type Type, string? Name);
    private readonly Dictionary<ParameterKey, TableInfo> lambdaParameterInfos = [];
    private string? parameterPrefix;
    public string? ParameterPrefix => parameterPrefix;
    public IDatabaseAdapter Database { get; }

    private readonly ResolveContext? parent;
    private readonly List<TableInfo> selectedTables;

    public int Depth { get; set; }

    public ResolveContext(IDatabaseAdapter database, params List<TableInfo> selectedTables)
    {
        Database = database;
        this.selectedTables = selectedTables;
    }

    public ResolveContext(ResolveContext upperContext, params List<TableInfo> selectedTables)
    {
        Database = upperContext.Database;
        Depth = upperContext.Depth + 1;
        this.selectedTables = selectedTables;
        parent = upperContext;
    }

    public static ResolveContext Create(DbBaseType type)
    {
        return new ResolveContext(type.GetDbCustom());
    }
    public void SetParamPrefix(string? parameterPrefix)
    {
        this.parameterPrefix = $"{parameterPrefix}_";
    }
    //internal void ModifyAlias(Action<ITableEntityInfo> action) => selectedTables.ForEach(action);
    public void HandleParameterExpression(ParameterExpression pExp, int index)
    {
        //var key = $"{pExp.Type}_{pExp.Name}";
        var key = new ParameterKey(pExp.Type, pExp.Name);
        if (!lambdaParameterInfos.TryGetValue(key, out var p))
        {
            p = selectedTables.FirstOrDefault(t => t.Type == pExp.Type && t.Index == index) ?? TableInfo.Create(pExp.Type, index);
            p.Parameter = pExp;
            p.Name = pExp.Name;
            lambdaParameterInfos.Add(key, p);
        }
        p.Depth = Depth;
    }

    public TableInfo GetTable(Func<ParameterKey, bool> predicate)
    {
        foreach (var item in lambdaParameterInfos)
        {
            if (predicate.Invoke(item.Key))
                return item.Value;
        }
        if (parent is not null)
        {
            return parent.GetTable(predicate);
        }
        throw new LightOrmException("解析ParameterExpression出错");
    }

    public TableInfo GetTable(ParameterExpression pExp)
    {
        var key = new ParameterKey(pExp.Type, pExp.Name);
        if (lambdaParameterInfos.TryGetValue(key, out var ti))
        {
            return ti;
        }
        if (parent is not null)
        {
            return parent.GetTable(pExp);
        }
        throw new LightOrmException("解析ParameterExpression出错");
    }
}

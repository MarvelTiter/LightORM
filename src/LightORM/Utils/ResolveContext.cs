using LightORM.Extension;

namespace LightORM;

public class ResolveContext
{
    readonly record struct ParameterKey(Type Type, string? Name);

    private readonly Dictionary<ParameterKey, TableInfo> lambdaParameterInfos = [];
    private string? parameterPrefix;
    public string? ParameterPrefix => parameterPrefix;
    public IDatabaseAdapter Database { get; }

    private readonly ResolveContext? parent;

    public int Level { get; set; }
    
    public ResolveContext(IDatabaseAdapter database)
    {
        Database = database;
    }

    public ResolveContext(ResolveContext upperContext)
    {
        Database = upperContext.Database;
        Level = upperContext.Level + 1;
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
            p = TableInfo.Create(pExp.Type, index);
            p.Name = pExp.Name;
            lambdaParameterInfos.Add(key, p);
        }
        p.Deep = Level;
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

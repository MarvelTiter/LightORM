using LightORM.Extension;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Reflection;

namespace LightORM;

internal class ResolveContext
{
    private readonly ConcurrentDictionary<string, AnonymousMapInfo> anonymousMap = new();

    private readonly List<ITableEntityInfo> selectedTables = [];
    private readonly Dictionary<string, TableInfo> lambdaParameterInfos = [];
    private string? parameterPrefix;
    public string? ParameterPrefix => parameterPrefix;
    public ICustomDatabase Database { get; }
    public IEnumerable<TableInfo> Tables => lambdaParameterInfos.Values;
    public int Level { get; set; }
    public ResolveContext(ICustomDatabase database, params ITableEntityInfo[] selectedTables)
    {
        //foreach (var item in selectedTables)
        //{
        //    this.selectedTables.Add(item.Type!, item);
        //}
        Database = database;
        this.selectedTables.AddRange(selectedTables);
    }
    public ResolveContext(ICustomDatabase database)
    {
        Database = database;
    }
    public static ResolveContext Create(DbBaseType type, params ITableEntityInfo[] selectedTables)
    {
        return new ResolveContext(type.GetDbCustom(), selectedTables);
    }
    public void SetParamPrefix(string? parameterPrefix)
    {
        this.parameterPrefix = $"{parameterPrefix}_";
    }
    //internal void ModifyAlias(Action<ITableEntityInfo> action) => selectedTables.ForEach(action);
    public void HandleParameterExpression(ParameterExpression pExp, int index)
    {
        var key = $"{pExp.Type.Name}_{pExp.Name}";
        if (!lambdaParameterInfos.TryGetValue(key, out var p))
        {
            p = TableInfo.Create(pExp.Type, index);
            p.Name = pExp.Name;
            lambdaParameterInfos.Add(key, p);
        }
    }
    //public void AddSelectedTable(ParameterExpression parameter)
    //{
    //    //if (!selectedTables.TryGetValue(parameter.Type, out var info))
    //    //{
    //    //    info = TableContext.GetTableInfo(parameter.Type);
    //    //    selectedTables.Add(parameter.Type, info);
    //    //}
    //    //info.Alias ??= parameter.Name;
    //    var type = parameter.Type;
    //    if (!selectedTables.Any(t => t.Type == type || type.IsAssignableFrom(t.Type)))
    //    {
    //        var table = TableContext.GetTableInfo(type);
    //        if (Level > 0)
    //        {
    //            table.Alias = table.Alias?.Replace("a", $"s{Level}_");
    //        }
    //        selectedTables.Add(table);
    //    }
    //}

    public void AddSelectedTable(ITableEntityInfo table)
    {
        if (!selectedTables.Any(t => t.Type == table.Type || table.Type!.IsAssignableFrom(t.Type)))
        {
            selectedTables.Add(table);
        }
    }

    public ITableEntityInfo GetTable(Type type)
    {
        var table = selectedTables.FirstOrDefault(t => t.Type == type || type.IsAssignableFrom(t.Type));
        //ArgumentNullException.ThrowIfNull(table, $"当前作用域中未找到类型`{type.Name}`的ITableEntityInfo");
        table ??= TableContext.GetTableInfo(type);
        return table;
    }

    public TableInfo GetTable(ParameterExpression pe)
    {
        //var ti = lambdaParameterInfos.FirstOrDefault(p => p.Name == pe.Name && p.Type == pe.Type) ?? 
        //return ti;

        var key = $"{pe.Type.Name}_{pe.Name}";
        if (lambdaParameterInfos.TryGetValue(key, out var ti))
        {
            return ti;
        }
        throw new LightOrmException("解析ParameterExpression出错");
    }

    public void CreateAnonymousMap(Type anonymousType, ParameterExpression paramExp, string anonymousName, string originName)
    {
        var originType = paramExp.Type;
        var oldKey = $"{originType.GUID}_{originName}";
        var newKey = $"{anonymousType.GUID}_{anonymousName}";
        var newInfo = new AnonymousMapInfo(originType, originName, paramExp);
        System.Diagnostics.Debug.WriteLine($"Create Map {anonymousType.Name}_{anonymousName}");
        System.Diagnostics.Debug.WriteLine($"From {originType.Name}_{originName}");
        if (anonymousMap.TryGetValue(oldKey, out var info))
        {
            newInfo.Parent = info;
            //anonymousMap.Remove(oldKey);
        }
        anonymousMap.TryAdd(newKey, newInfo);
    }
    public bool TryGetAnonymousInfo(ParameterExpression? paramExp, string anonymousName, out AnonymousMapInfo? member)
    {
        var anonymousType = paramExp?.Type;
        if (anonymousType is null)
        {
            member = null;
            return false;
        }
        var key = $"{anonymousType.GUID}_{anonymousName}";
        anonymousMap.TryGetValue(key, out member);
        if (member == null)
        {
            //LightOrmException.Throw($"获取匿名类型映射错误, 不存在该类型的映射`{anonymousType.FullName}.{anonymousName}`");
            return false;
        }
        return true;
    }

    public class AnonymousMapInfo(Type map, string name, ParameterExpression pExp)
    {
        public AnonymousMapInfo? Parent { get; set; }
        public Type MapType { get; } = map;
        public ParameterExpression ParameterExp { get; } = pExp;
        public string Name { get; } = name;
    }
}

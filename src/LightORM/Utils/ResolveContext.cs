using LightORM.Extension;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Reflection;

namespace LightORM;

public partial class ResolveContext
{

    private readonly List<ITableEntityInfo> selectedTables = [];
    private string? parameterPrefix;
    public string? ParameterPrefix => parameterPrefix;
    public ICustomDatabase Database { get; }
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
    public static ResolveContext Create(DbBaseType type, params ITableEntityInfo[] selectedTables)
    {
        return new ResolveContext(type.GetDbCustom(), selectedTables);
    }
    public void SetParamPrefix(string? parameterPrefix)
    {
        this.parameterPrefix = $"{parameterPrefix}_";
    }
    internal void ModifyAlias(Action<ITableEntityInfo> action) => selectedTables.ForEach(action);

    public void AddSelectedTable(ParameterExpression parameter)
    {
        //if (!selectedTables.TryGetValue(parameter.Type, out var info))
        //{
        //    info = TableContext.GetTableInfo(parameter.Type);
        //    selectedTables.Add(parameter.Type, info);
        //}
        //info.Alias ??= parameter.Name;
        var type = parameter.Type;
        if (!selectedTables.Any(t => t.Type == type || type.IsAssignableFrom(t.Type)))
        {
            var table = TableContext.GetTableInfo(type);
            if (Level > 0)
            {
                table.Alias = table.Alias?.Replace("a", $"s{Level}_");
            }
            selectedTables.Add(table);
        }
    }

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
}

/// <summary>
/// 匿名属性映射
/// </summary>
public partial class ResolveContext
{
    class AnonymousMapInfo(Type map, string name)
    {
        public AnonymousMapInfo? Parent { get; set; }
        public Type MapType { get; } = map;
        public string Name { get; } = name;
    }
    private readonly Dictionary<string, AnonymousMapInfo> anonymousMap = new();
    public void CreateAnonymousMap(Type anonymousType, Type originType, string anonymousName, string originName)
    {
        //var key = $"{anonymousType.FullName}_{anonymousName}";
        //var originMember = originType.GetMember(originName).First(m => m.MemberType == MemberTypes.Property);
        //anonymousMap.TryAdd(key, originMember);
        var oldKey = $"{originType.GUID}_{originName}";
        var newKey = $"{anonymousType.GUID}_{anonymousName}";
        var newInfo = new AnonymousMapInfo(originType, originName);
        if (anonymousMap.TryGetValue(oldKey, out var info))
        {
            newInfo.Parent = info;
            anonymousMap.Remove(oldKey);
        }
        anonymousMap.Add(newKey, newInfo);
    }
    public bool GetAnonymousInfo(Type anonymousType, string anonymousName, out MemberInfo? member)
    {
        //var key = $"{anonymousType.FullName}_{anonymousName}";
        //anonymousMap.TryGetValue(key, out member);
        //if (member == null)
        //{
        //    //LightOrmException.Throw($"获取匿名类型映射错误, 不存在该类型的映射`{anonymousType.FullName}.{anonymousName}`");
        //    return false;
        //}
        //return true;

        var key = $"{anonymousType.GUID}_{anonymousName}";
        if (anonymousMap.TryGetValue(key, out var info))
        {
            while (info is not null)
            {
                if (info.Parent is not null)
                {
                    info = info.Parent;
                    continue;
                }
                member = info.MapType.GetMember(info.Name).First(m => m.MemberType == MemberTypes.Property);
                return true;
            }
        }
        member = null;
        return false;
    }
}

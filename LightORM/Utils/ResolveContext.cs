using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Reflection;

namespace LightORM;

public class ResolveContext
{
    private readonly ConcurrentDictionary<string, MemberInfo> anonymousMap = new();

    private readonly List<ITableEntityInfo> selectedTables = [];
    private string? parameterPrefix;
    public string? ParameterPrefix => parameterPrefix;
    public ResolveContext(params ITableEntityInfo[] selectedTables)
    {
        //foreach (var item in selectedTables)
        //{
        //    this.selectedTables.Add(item.Type!, item);
        //}
        this.selectedTables.AddRange(selectedTables);
    }

    public void SetParamPrefix(string? parameterPrefix)
    {
        this.parameterPrefix = $"{parameterPrefix}_";
    }

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
            selectedTables.Add(TableContext.GetTableInfo(type));
        }
    }

    public ITableEntityInfo GetTable(Type type)
    {
        var table = selectedTables.FirstOrDefault(t => t.Type == type || type.IsAssignableFrom(t.Type));
        //ArgumentNullException.ThrowIfNull(table, $"当前作用域中未找到类型`{type.Name}`的ITableEntityInfo");
        table ??= TableContext.GetTableInfo(type);
        return table;
    }

    public void CreateAnonymousMap(Type anonymousType, Type originType, string anonymousName, string originName)
    {
        var key = $"{anonymousType.FullName}_{anonymousName}";
        var originMember = originType.GetMember(originName).First(m => m.MemberType == MemberTypes.Property);
        anonymousMap.TryAdd(key, originMember);
    }
    public MemberInfo GetAnonymousInfo(Type anonymousType, string anonymousName)
    {
        var key = $"{anonymousType.FullName}_{anonymousName}";
        anonymousMap.TryGetValue(key, out var member);
        ArgumentNullException.ThrowIfNull(member, $"获取匿名类型映射错误, 不存在该类型的映射`{anonymousType.FullName}.{anonymousName}`");
        return member;
    }
}

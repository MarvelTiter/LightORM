﻿using LightORM.Extension;
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

    public void CreateAnonymousMap(Type anonymousType, Type originType, string anonymousName, string originName)
    {
        var key = $"{anonymousType.FullName}_{anonymousName}";
        var originMember = originType.GetMember(originName).First(m => m.MemberType == MemberTypes.Property);
        anonymousMap.TryAdd(key, originMember);
    }
    public bool GetAnonymousInfo(Type anonymousType, string anonymousName, out MemberInfo? member)
    {
        var key = $"{anonymousType.FullName}_{anonymousName}";
        anonymousMap.TryGetValue(key, out member);
        if (member == null)
        {
            //LightOrmException.Throw($"获取匿名类型映射错误, 不存在该类型的映射`{anonymousType.FullName}.{anonymousName}`");
            return false;
        }
        return true;
    }
}

using LightORM.Utils;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace LightORM.Models;
internal record TableEntity : ITableEntityInfo
{
    public TableEntity(Type type)
    {
        Type = type;
        IsAnonymousType = type.FullName?.StartsWith("<>f__AnonymousType") ?? false;
    }

    public Type Type { get; set; }
    public string TableName => CustomName ?? Type?.Name ?? throw new LightOrmException("获取表名异常");
    public string? Alias { get; set; }
    public bool IsAnonymousType { get; set; }
    public string? CustomName { get; set; }
    public string? TargetDatabase { get; set; }
    public string? Description { get; set; }
    public ITableColumnInfo[] Columns { get; set; } = [];

    private static ConcurrentDictionary<string, Func<object, object?>> getters = [];
    private static ConcurrentDictionary<string, Action<object, object?>> setters = [];

    public object? GetValue(ITableColumnInfo col, object target)
    {
        return getters.GetOrAdd(col.PropertyName, key =>
        {
            var prop = Type.GetProperty(col.PropertyName);
            return Type.GetPropertyAccessor(prop);
        }).Invoke(target);
    }

    public void SetValue(ITableColumnInfo col, object target, object? value)
    {
        setters.GetOrAdd(col.PropertyName, key =>
        {
            var prop = Type.GetProperty(col.PropertyName);
            return Type.GetPropertySetter(prop);
        }).Invoke(target, value);
    }

    //public object MapDataReader(IDataReader reader)
    //{
    //    var u = new Test();
    //    string[] columns = new string[reader.FieldCount];
    //    for (int i = 0; i < reader.FieldCount; i++)
    //    {
    //        columns[i] = reader.GetName(i);
    //    }
        
    //}

    //class Test
    //{
    //    public int Age { get; set; }
    //    public int Level { get; set; }
    //}
}

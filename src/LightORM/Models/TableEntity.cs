using LightORM.Extension;
using System.Collections.Concurrent;

namespace LightORM.Models;
internal record TableEntity : ITableEntityInfo
{
    public TableEntity(Type type)
    {
        Type = type;
        IsAnonymousType = type.IsAnonymous();
    }
    public TableEntity()
    {

    }
    public Type? Type { get; set; }
    public string TableName => CustomName ?? Type?.Name ?? throw new LightOrmException("获取表名异常");
    public string? Schema { get; set; }
    public string? Alias { get; set; }
    public bool IsAnonymousType { get; set; }
    public bool IsTempTable { get; set; }
    public string? CustomName { get; set; }
    public string? TargetDatabase { get; set; }
    public string? Description { get; set; }
    public ITableColumnInfo[] Columns { get; set; } = [];

    //private readonly ConcurrentDictionary<string, ITableColumnInfo> columnMap = new();
    //public ITableColumnInfo GetColumnInfo(string propertyName)
    //{
    //    if (!columnMap.TryGetValue(propertyName, out var column))
    //    {
    //        column = Columns.First(i => i.PropertyName == propertyName);
    //        columnMap.TryAdd(propertyName, column);
    //    }
    //    return column;
    //}
}

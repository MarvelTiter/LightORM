using System;
using System.Collections.Generic;
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
    public TableEntity()
    {

    }
    public Type? Type { get; set; }
    public string TableName => CustomName ?? Type?.Name ?? throw new LightOrmException("获取表名异常");
    public string? Alias { get; set; }
    public bool IsAnonymousType { get; set; }
    public string? CustomName { get; set; }
    public string? TargetDatabase { get; set; }
    public string? Description { get; set; }
    public ColumnInfo[] Columns { get; set; } = [];

    public object? GetValue(ColumnInfo col, object target)
    {
        throw new NotImplementedException();
    }

    public void SetValue(ColumnInfo col, object target, object? value)
    {
        throw new NotImplementedException();
    }
}

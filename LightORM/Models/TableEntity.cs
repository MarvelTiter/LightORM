using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LightORM.Models;

internal record TableEntity
{
    public TableEntity(Type type)
    {
        Type = type;
        IsAnonymousType = type.FullName?.StartsWith("<>f__AnonymousType") ?? false;
    }
    public TableEntity()
    {
        
    }
    public Type? Type { get; internal set; }
    public string TableName => CustomName ?? Type?.Name ?? throw new ArgumentNullException();
    public string? Alias { get; internal set; }
    public bool IsAnonymousType { get; internal set; }
    public string? CustomName { get; internal set; }
    public string? TargetDatabase { get; internal set; }
    public string? Description { get; internal set; }
    public List<ColumnInfo> Columns { get; internal set; } = [];
}

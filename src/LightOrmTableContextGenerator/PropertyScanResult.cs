using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.CodeAnalysis;

namespace LightOrmTableContextGenerator;

internal record struct PropertyScanResult
{
    public IPropertySymbol Symbol { get; set; }
    public string PropertyName { get; set; }
    public string CustomName { get; set; }
    public string PrimaryKey { get; set; }
    public string IsNotMap { get; set; }
    public string AutoIncrement { get; set; }
    public string NotNull { get; set; }
    public string? Len { get; set; }
    public string? DefaultValue { get; set; }
    public string? Comment { get; set; }
    public string CanRead { get; set; }
    public string CanWrite { get; set; }
    public string CanInit { get; set; }
    public NavigateContext? NavInfo { get; set; }
    public string IsVersion { get; set; }
    public string IgnoreUpdate { get; set; }
    public string IgnoreInsert { get; set; }
    public string IsJson { get; set; }
    public bool IsFlat { get; set; }
    public bool IsFlatProperty { get; set; }
    public string? FlatType { get; set; }
}

internal readonly record struct NavigateContext(ITypeSymbol TargetType, string MainName, string SubName, bool IsMultiResult, ITypeSymbol? MappingType = null)
{
    public override string ToString()
    {
        if (TargetType is null)
        {
            return "null";
        }
        var mpt = MappingType is null ? "null" : $"typeof({MappingType.ToDisplayString()})";
        var multi = IsMultiResult ? "true" : "false";
        return $"new global::LightORM.Models.NavigateInfo(typeof({TargetType.ToDisplayString()}), {mpt}, \"{MainName}\", \"{SubName}\", {multi})";
    }
}
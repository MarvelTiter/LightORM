using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using LightORM.Extension;
using System.Xml.Linq;

namespace LightORM.Models;

internal record ColumnInfo
{
    public TableEntity Table { get; }
    public string ColumnName => CustomName ?? Property.Name;
    public string? CustomName { get; set; }
    public bool? AutoIncrement { get; }
    public bool? NotNull { get; }
    public int? Length { get; }
    public object? Default { get; }
    public string? Comment { get; }
    public PropertyInfo Property { get; }
    public Type PropertyType { get; }
    public Type? UnderlyingType { get; }
    public bool IsNullable { get; }
#if NET6_0_OR_GREATER
    public System.ComponentModel.DataAnnotations.Schema.DatabaseGeneratedOption? DatabaseGeneratedOption { get; set; }
#endif
    public bool IsNotMapped { get; set; }
    public bool IsPrimaryKey { get; set; }
    public ColumnInfo(TableEntity table, PropertyInfo property)
    {
        Table = table;
        Property = property;
        PropertyType = property.PropertyType;
        var underlying = Nullable.GetUnderlyingType(property.PropertyType);
        UnderlyingType = underlying ?? property.PropertyType;
        IsNullable = underlying != null;

        var lightColAttr = property.GetAttribute<LightColumnAttribute>();
#if NET6_0_OR_GREATER
        var databaseGeneratedAttribute = property.GetCustomAttribute<System.ComponentModel.DataAnnotations.Schema.DatabaseGeneratedAttribute>(false);
        if (databaseGeneratedAttribute != null)
        {
            DatabaseGeneratedOption = databaseGeneratedAttribute.DatabaseGeneratedOption;
        }
        var colAttr = property.GetAttribute<System.ComponentModel.DataAnnotations.Schema.ColumnAttribute>();
        IsNotMapped = property.HasAttribute<System.ComponentModel.DataAnnotations.Schema.NotMappedAttribute>() || property.HasAttribute<IgnoreAttribute>();
        IsPrimaryKey = property.HasAttribute<System.ComponentModel.DataAnnotations.KeyAttribute>() || (lightColAttr?.PrimaryKey ?? false);
#else
        IsNotMapped = property.HasAttribute<IgnoreAttribute>();
        IsPrimaryKey = (lightColAttr?.PrimaryKey ?? false);
#endif
        CustomName = lightColAttr?.Name;
        AutoIncrement = lightColAttr?.AutoIncrement;
        NotNull = lightColAttr?.NotNull;
        Length = lightColAttr?.Length;
        Default = lightColAttr?.Default;
        Comment = lightColAttr?.Comment;

    }
}


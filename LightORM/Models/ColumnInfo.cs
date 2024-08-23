using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using LightORM.Extension;
using System.Xml.Linq;
using LightORM.Utils;

namespace LightORM.Models;
public sealed record ColumnInfo
{
    public ITableEntityInfo Table { get; }
    public string ColumnName => CustomName ?? Property.Name;
    public string PropName => Property.Name;
    public string? CustomName { get; set; }
    public bool? AutoIncrement { get; }
    public bool? NotNull { get; }
    public int? Length { get; }
    public object? Default { get; }
    public string? Comment { get; }
    public PropertyInfo Property { get; }
    public Type PropertyType { get; }
    public Type UnderlyingType { get; }
    public bool IsNullable { get; }
    public bool IsNavigate { get; set; }
    public NavigateInfo? NavigateInfo { get; set; }

#if NET6_0_OR_GREATER
    public System.ComponentModel.DataAnnotations.Schema.DatabaseGeneratedOption? DatabaseGeneratedOption { get; set; }
#endif
    public bool IsNotMapped { get; set; }
    public bool IsPrimaryKey { get; set; }
    readonly Func<object, object>? valueGetter;
    readonly Action<object, object>? valueSetter;
    public object? GetValue(object target) => valueGetter?.Invoke(target);
    public void SetValue(object target, object value) => valueSetter?.Invoke(target, value);
    public ColumnInfo(ITableEntityInfo table, PropertyInfo property)
    {
        Table = table;
        Property = property;
        PropertyType = property.PropertyType;
        var underlying = Nullable.GetUnderlyingType(property.PropertyType);
        UnderlyingType = underlying ?? property.PropertyType;
        IsNullable = underlying != null;

        valueGetter = table.Type?.GetPropertyAccessor(property);

        var lightColAttr = property.GetAttribute<LightColumnAttribute>();
        var oldColAttr = property.GetAttribute<ColumnAttribute>();
#if NET6_0_OR_GREATER
        var databaseGeneratedAttribute = property.GetCustomAttribute<System.ComponentModel.DataAnnotations.Schema.DatabaseGeneratedAttribute>(false);
        if (databaseGeneratedAttribute != null)
        {
            DatabaseGeneratedOption = databaseGeneratedAttribute.DatabaseGeneratedOption;
        }
        var colAttr = property.GetAttribute<System.ComponentModel.DataAnnotations.Schema.ColumnAttribute>();
        IsNotMapped = property.HasAttribute<System.ComponentModel.DataAnnotations.Schema.NotMappedAttribute>() || property.HasAttribute<IgnoreAttribute>();
        IsPrimaryKey = property.HasAttribute<System.ComponentModel.DataAnnotations.KeyAttribute>() || (lightColAttr?.PrimaryKey ?? false);
        CustomName = lightColAttr?.Name ?? colAttr?.Name ?? oldColAttr?.Name;
#else
        IsNotMapped = property.HasAttribute<IgnoreAttribute>();
        IsPrimaryKey = (lightColAttr?.PrimaryKey ?? false);
        CustomName = lightColAttr?.Name ?? oldColAttr?.Name;
#endif

        AutoIncrement = lightColAttr?.AutoIncrement;
        NotNull = lightColAttr?.NotNull;
        Length = lightColAttr?.Length;
        Default = lightColAttr?.Default;
        Comment = lightColAttr?.Comment;

        var navigateInfo = property.GetAttribute<LightNavigate>();
        if (navigateInfo != null)
        {
            IsNavigate = true;
            Type elType = property.PropertyType.GetRealType(out var multi);
            NavigateInfo = new NavigateInfo(elType)
            {
                MappingType = navigateInfo.ManyToMany,
                MainName = navigateInfo.MainName,
                SubName = navigateInfo.SubName,
                IsMultiResult = multi,
            };
            valueSetter = table.Type?.GetPropertySetter(property);
        }

    }
}


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

internal sealed record NavigateInfo
{
    public NavigateInfo(Type mainType)
    {
        NavigateType = mainType;
    }
    /// <summary>
    /// 多对多类型
    /// </summary>
    public Type NavigateType { get; }
    /// <summary>
    /// 多对多关联表
    /// </summary>
    public Type? MappingType { get; set; }

    public string? MainName { get; set; }
    public string? SubName { get; set; }

}
internal sealed record ColumnInfo
{
    public TableEntity Table { get; }
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
    public Type? UnderlyingType { get; }
    public bool IsNullable { get; }
    public bool IsNavigate { get; set; }
    public NavigateInfo? NavigateInfo { get; set; }

#if NET6_0_OR_GREATER
    public System.ComponentModel.DataAnnotations.Schema.DatabaseGeneratedOption? DatabaseGeneratedOption { get; set; }
#endif
    public bool IsNotMapped { get; set; }
    public bool IsPrimaryKey { get; set; }
    readonly Func<object, object>? valueGetter;
    public object? GetValue(object target) => valueGetter?.Invoke(target);
    public ColumnInfo(TableEntity table, PropertyInfo property)
    {
        Table = table;
        Property = property;
        PropertyType = property.PropertyType;
        var underlying = Nullable.GetUnderlyingType(property.PropertyType);
        UnderlyingType = underlying ?? property.PropertyType;
        IsNullable = underlying != null;

        valueGetter = table.Type?.GetPropertyAccessor(property);

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

        var navigateInfo = property.GetAttribute<LightNavigate>();
        if (navigateInfo != null)
        {
            IsNavigate = true;
            Type elType = property.PropertyType;
            if (property.PropertyType.IsArray)
            {
                elType = property.PropertyType.GetElementType();
            }
            else if (property.PropertyType.IsGenericType)
            {
                elType = property.PropertyType.GetGenericArguments()[0];
            }
            NavigateInfo = new NavigateInfo(elType)
            {
                MappingType = navigateInfo.ManyToMany,
                MainName = navigateInfo.MainName,
                SubName = navigateInfo.SubName,
            };
        }

    }
}


using System.Reflection;
using LightORM.Extension;

namespace LightORM.Models;

public static class ColumnInfoExtensions
{
    public static object? GetValue(this ITableColumnInfo col, object target)
    {
        //Console.WriteLine($"{col.PropertyName} -> {col.IsAggregated} -> {col.IsAggregatedProperty}");
        return col.IsAggregated ? null : TableContext.GetValue(col, target);
    }
    public static void SetValue(this ITableColumnInfo col, object target, object? value)
    {
        if (col.IsAggregated)
        {
            return;
        }
        TableContext.SetValue(col, target, value);
    }
}
public sealed record ColumnInfo : ITableColumnInfo
{
    //public ITableEntityInfo Table { get; set; }
    public Type TableType { get; }
    public string ColumnName => CustomName ?? PropertyName;
    public string PropertyName { get; set; }
    public string? CustomName { get; set; }
    public bool AutoIncrement { get; set; }
    public bool NotNull { get; set; }
    public int? Length { get; set; }
    public object? Default { get; set; }
    public string? Comment { get; set; }
    public bool IsVersionColumn { get; set; }
    public bool IsIgnoreUpdate { get; set; }
    public bool IsIgnoreInsert { get; set; }
    //public PropertyInfo Property { get; set; }
    //public Type PropertyType { get; set; }
    //public Type UnderlyingType { get; set; }
    public bool IsNullable { get; set; }
    public bool IsNavigate { get; set; }
    public NavigateInfo? NavigateInfo { get; set; }

    //#if NET6_0_OR_GREATER
    //    public System.ComponentModel.DataAnnotations.Schema.DatabaseGeneratedOption? DatabaseGeneratedOption { get; set; }
    //#endif
    public bool IsNotMapped { get; set; }
    public bool IsPrimaryKey { get; set; }

    public bool CanRead { get; set; }
    public bool CanWrite { get; set; }
    public bool CanInit { get; set; }
    public Type? AggregateType { get; }
    public bool IsAggregated { get; }
    public bool IsAggregatedProperty { get; }

    public string? AggregateProp { get; set; }

    //public object? GetValue(object target) => throw new Exception();//Table.GetValue(this, target);
    //public void SetValue(object target, object value) => throw new Exception();// Table.SetValue(this, target, value);
    public ColumnInfo(Type owner
        , string propname
        , string? customname
        , bool isprimary
        , bool isnotmap
        , bool isAutoIncrement
        , bool isNotNull
        , int? length
        , object? defaultValue
        , string? comment
        , bool canRead
        , bool canWrite
        , bool canInit
        , NavigateInfo? navigationInfo
        , Type? aggregateType
        , bool isAggregated
        , bool isAggregaredProp
        , bool isVersionColumn
        , bool isIgnoreUpdate
        , bool isIgnoreInsert
        )
    {
        TableType = owner;
        PropertyName = propname;
        CustomName = customname;
        IsPrimaryKey = isprimary;
        IsNotMapped = isnotmap;

        AutoIncrement = isAutoIncrement;
        NotNull = isNotNull;
        Length = length;
        Default = defaultValue;
        Comment = comment;
        CanRead = canRead;
        CanWrite = canWrite;
        CanInit = canInit;
        if (navigationInfo != null)
        {
            IsNavigate = true;
            NavigateInfo = navigationInfo;
        }
        AggregateType = aggregateType;
        IsAggregated = isAggregated;
        IsAggregatedProperty = isAggregaredProp;
        IsVersionColumn = isVersionColumn;
        IsIgnoreUpdate = isIgnoreUpdate;
        IsIgnoreInsert = isIgnoreInsert;
    }

    public ColumnInfo(Type owner, PropertyInfo property, Type? aggregateType, bool isAggregated, bool isAggregaredProp)
    {
        TableType = owner;
        PropertyName = property.Name;
        //Property = property;
        //PropertyType = property.PropertyType;
        var underlying = Nullable.GetUnderlyingType(property.PropertyType);
        //UnderlyingType = underlying ?? property.PropertyType;
        CanRead = property.CanRead;
        CanWrite = property.CanWrite;
        CanInit = property.SetMethod is not null;
        IsNullable = underlying != null;

        var lightColAttr = property.GetAttribute<LightColumnAttribute>();
#if NET6_0_OR_GREATER
        //var databaseGeneratedAttribute = property.GetCustomAttribute<System.ComponentModel.DataAnnotations.Schema.DatabaseGeneratedAttribute>(false);
        //if (databaseGeneratedAttribute != null)
        //{
        //    DatabaseGeneratedOption = databaseGeneratedAttribute.DatabaseGeneratedOption;
        //}
        var colAttr = property.GetAttribute<System.ComponentModel.DataAnnotations.Schema.ColumnAttribute>();
        IsNotMapped = lightColAttr?.Ignore ?? property.HasAttribute<System.ComponentModel.DataAnnotations.Schema.NotMappedAttribute>();
        IsPrimaryKey = property.HasAttribute<System.ComponentModel.DataAnnotations.KeyAttribute>() || (lightColAttr?.PrimaryKey ?? false);
        CustomName = lightColAttr?.Name ?? colAttr?.Name;
#else
        IsNotMapped = lightColAttr?.Ignore ?? false;
        IsPrimaryKey = lightColAttr?.PrimaryKey ?? false;
        CustomName = lightColAttr?.Name;
#endif

        AutoIncrement = lightColAttr?.AutoIncrement ?? false;
        NotNull = lightColAttr?.NotNull ?? false;
        Length = lightColAttr?.Length > 0 ? lightColAttr.Length : null;
        Default = lightColAttr?.Default;
        Comment = lightColAttr?.Comment;
        IsVersionColumn = lightColAttr?.Version ?? false;
        IsIgnoreUpdate = lightColAttr?.IgnoreUpdate ?? false;
        IsIgnoreInsert = lightColAttr?.IgnoreInsert ?? false;

        var navigateInfo = property.GetAttribute<LightNavigateAttribute>();
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
        }
        AggregateType = aggregateType;
        IsAggregated = isAggregated;
        IsAggregatedProperty = isAggregaredProp;
    }
}


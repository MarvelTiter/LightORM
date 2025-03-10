using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using LightORM.Extension;
using System.Xml.Linq;
using LightORM.Utils;
using System.Diagnostics.CodeAnalysis;

namespace LightORM.Models;

public sealed record ColumnInfo : ITableColumnInfo
{
    public ITableEntityInfo Table { get; set; }
    public string ColumnName => CustomName ?? PropertyName;
    public string PropertyName { get; set; }
    public string? CustomName { get; set; }
    public bool AutoIncrement { get; set; }
    public bool NotNull { get; set; }
    public int? Length { get; set; }
    public object? Default { get; set; }
    public string? Comment { get; set; }
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
    public object? GetValue(object target) => throw new Exception();//Table.GetValue(this, target);
    public void SetValue(object target, object value) => throw new Exception();// Table.SetValue(this, target, value);
    public ColumnInfo(ITableEntityInfo table
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
        )
    {
        Table = table;
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
    }

    public ColumnInfo(ITableEntityInfo table, PropertyInfo property)
    {
        Table = table;
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
        var oldColAttr = property.GetAttribute<ColumnAttribute>();
#if NET6_0_OR_GREATER
        //var databaseGeneratedAttribute = property.GetCustomAttribute<System.ComponentModel.DataAnnotations.Schema.DatabaseGeneratedAttribute>(false);
        //if (databaseGeneratedAttribute != null)
        //{
        //    DatabaseGeneratedOption = databaseGeneratedAttribute.DatabaseGeneratedOption;
        //}
        var colAttr = property.GetAttribute<System.ComponentModel.DataAnnotations.Schema.ColumnAttribute>();
        IsNotMapped = property.HasAttribute<System.ComponentModel.DataAnnotations.Schema.NotMappedAttribute>() || property.HasAttribute<IgnoreAttribute>();
        IsPrimaryKey = property.HasAttribute<System.ComponentModel.DataAnnotations.KeyAttribute>() || (lightColAttr?.PrimaryKey ?? false);
        CustomName = lightColAttr?.Name ?? colAttr?.Name ?? oldColAttr?.Name;
#else
        IsNotMapped = property.HasAttribute<IgnoreAttribute>();
        IsPrimaryKey = (lightColAttr?.PrimaryKey ?? false);
        CustomName = lightColAttr?.Name ?? oldColAttr?.Name;
#endif

        AutoIncrement = lightColAttr?.AutoIncrement ?? false;
        NotNull = lightColAttr?.NotNull ?? false;
        Length = lightColAttr?.Length;
        Default = lightColAttr?.Default;
        Comment = lightColAttr?.Comment;

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
    }
}


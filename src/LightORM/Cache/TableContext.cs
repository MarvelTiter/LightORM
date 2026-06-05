using LightORM.Extension;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System;
using System.Runtime.CompilerServices;
namespace LightORM.Cache;

internal class AbstractTableType(Type type)
{
    public Type Type { get; } = type;
}
internal static partial class TableContext
{
    internal static ITableContext? StaticContext { get; set; }
    private static readonly ConcurrentDictionary<ITableColumnInfo, Action<object, object?>> reflectSetValueCaches = new(TableColumnCompare.Default);
    private static readonly ConcurrentDictionary<ITableColumnInfo, Func<object, object?>> reflectGetValueCaches = new(TableColumnCompare.Default);
    private static readonly ConcurrentDictionary<Type, TableEntity> reflectTables = [];
    private static readonly ConcurrentDictionary<Type, AbstractTableType> abstractTypeRels = [];

    public static ITableEntityInfo GetTableInfo<T>()
    {
        return GetTableInfo(typeof(T));
    }

    public static void SetValue(ITableColumnInfo col, object target, object? value)
    {
        var type = col.TableType;
        Action<ITableColumnInfo, object, object?>? action = StaticContext?.GetSetMethod(type);
        if (action != null)
        {
            action.Invoke(col, target, value);
        }
        else
        {
            //var key = $"{type.FullName}_{col.PropertyName}_Setter";
            var reflectAction = reflectSetValueCaches.GetOrAdd(col, static col =>
            {
                if (col.IsAggregatedProperty && col.AggregateType is not null)
                {
                    var property = col.AggregateType.GetProperty(col.PropertyName)!;
                    var aggregate = col.TableType.GetProperty(col.AggregateProp!)!;
                    return col.TableType.GetFlatPropertySetter(property, aggregate);
                }
                else
                {
                    var property = col.TableType.GetProperty(col.PropertyName)!;
                    return col.TableType.GetPropertySetter(property);
                }
            });
            reflectAction.Invoke(target, value);
        }
    }

    public static object? GetValue(ITableColumnInfo col, object target)
    {
        var type = col.TableType;
        Func<ITableColumnInfo, object, object?>? action = StaticContext?.GetGetMethod(type);
        if (action is not null)
        {
            return action.Invoke(col, target);
        }
        else
        {
            //var key = $"{type.FullName}_{col.PropertyName}_Getter";
            var reflectAction = reflectGetValueCaches.GetOrAdd(col, static col =>
            {
                if (col.IsAggregatedProperty && col.AggregateType is not null)
                {
                    var property = col.AggregateType.GetProperty(col.PropertyName)!;
                    var aggregate = col.TableType.GetProperty(col.AggregateProp!)!;
                    return col.TableType.GetFlatPropertyAccessor(property, aggregate);
                }
                else
                {
                    var property = col.TableType.GetProperty(col.PropertyName)!;
                    return col.TableType.GetPropertyAccessor(property);
                }
            });
            return reflectAction.Invoke(target);
        }
    }


    public static ITableEntityInfo GetTableInfo(Type type)
    {
        if (StaticContext != null)
        {
            try
            {
                var table = StaticContext.GetTableInfo(type);
                if (table != null)
                {
                    return table;
                }
            }
            catch { }
        }
        return GetTableInfoViaReflection(type);
    }

#if NET8_0_OR_GREATER
    [UnconditionalSuppressMessage("Trimming", "IL2075", Justification = "匿名类型的反射在 AOT 中安全，具名类型通过源生成器或运行时检测处理")]
    [UnconditionalSuppressMessage("Trimming", "IL2070", Justification = "匿名类型的反射在 AOT 中安全")]
    [UnconditionalSuppressMessage("Trimming", "IL2026", Justification = "匿名类型的反射在 AOT 中安全")]
#endif
    private static ITableEntityInfo GetTableInfoViaReflection(Type type)
    {
        EnsureReflectionAccess(type);

        var realType = type.GetRealType(out _);

        if (realType.IsAbstract || realType.IsInterface)
        {
            realType = abstractTypeRels.GetOrAdd(type, static t =>
            {
                var rt = (reflectTables.Values.FirstOrDefault(x => t.IsAssignableFrom(x.Type))?.Type) ?? throw new LightOrmException("无法解析的表");
                return new AbstractTableType(rt);
            }).Type;
            return GetTableInfoViaReflection(realType);
        }

        var entityInfoCache = reflectTables.GetOrAdd(type, static type =>
        {
            var entityInfo = new TableEntity(type);
            var lightTableAttribute = type.GetAttribute<LightTableAttribute>();
#if NET6_0_OR_GREATER
            var tableAttribute = type.GetAttribute<System.ComponentModel.DataAnnotations.Schema.TableAttribute>();
            entityInfo.CustomName = lightTableAttribute?.Name ?? tableAttribute?.Name ?? type.Name;
#else
            entityInfo.CustomName = lightTableAttribute?.Name ?? type.Name;
#endif
            if (!entityInfo.IsAnonymousType)
            {
                entityInfo.TargetDatabase = lightTableAttribute?.DatabaseKey;
                entityInfo.Schema = lightTableAttribute?.Schema;
                var descriptionAttribute = type.GetAttribute<DescriptionAttribute>();
                if (descriptionAttribute != null)
                {
                    entityInfo.Description = descriptionAttribute.Description;
                }
            }

            var propertyInfos = type.GetProperties();

            var propertyColumnInfos = propertyInfos.SelectMany(ScanProperty);
            entityInfo.Columns = [.. propertyColumnInfos];

            return entityInfo;
        });
        // 拷贝
        return entityInfoCache;
    }

#if NET8_0_OR_GREATER
    [RequiresUnreferencedCode("反射创建ColumnInfo不支持AOT，考虑使用LightOrmTableContextGenerator.TableContextGenerator生成器")]
#endif
    private static IEnumerable<ITableColumnInfo> ScanProperty(PropertyInfo prop)
    {
        if (prop.HasAttribute<LightFlatAttribute>())
        {
            var flatProps = prop.PropertyType.GetProperties();
            foreach (var item in flatProps)
            {
                yield return new ColumnInfo(prop.DeclaringType!, item, prop.PropertyType, false, true)
                {
                    AggregateProp = prop.Name
                };
            }
            yield return new ColumnInfo(prop.DeclaringType!, prop, prop.PropertyType, true, false);
        }
        else
        {
            yield return new ColumnInfo(prop.DeclaringType!, prop, null, false, false);
        }
    }
}

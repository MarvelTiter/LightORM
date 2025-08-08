using LightORM.Extension;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.Reflection;

namespace LightORM.Cache;

internal class AbstractTableType
{
    public AbstractTableType(Type type)
    {
        Type = type;
    }

    public Type Type { get; }
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
            var reflectAction = reflectGetValueCaches.GetOrAdd(col,static col =>
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
        var cacheKey = $"DbTable_{type.GUID}";

        var realType = type.GetRealType(out _);

        if (realType.IsAbstract || realType.IsInterface)
        {
            realType = abstractTypeRels.GetOrAdd(type, static t =>
            {
                var rt = reflectTables.Values.Where(x => t.IsAssignableFrom(x.Type)).FirstOrDefault()?.Type;
                if (rt is null) LightOrmException.Throw("无法解析的表");
                return new AbstractTableType(rt!);
            }).Type;
            return GetTableInfo(realType);
        }

        //if (realType.HasAttribute<LightFlatAttribute>())
        //{

        //}

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
                var descriptionAttribute = type.GetAttribute<DescriptionAttribute>();
                if (descriptionAttribute != null)
                {
                    entityInfo.Description = descriptionAttribute.Description;
                }
            }

            var propertyInfos = type.GetProperties();

            var propertyColumnInfos = propertyInfos.SelectMany(ScanProperty);
            entityInfo.Columns = [.. propertyColumnInfos];
            //if (entityInfo.IsAnonymousType)
            //{
            //    entityInfo.Alias = $"t{StaticCache<TableEntity>.Count}";
            //}
            //else
            //{
            //    entityInfo.Alias = $"r{StaticCache<TableEntity>.Count}";
            //}
            return entityInfo;
        });
        // 拷贝
        return entityInfoCache with { };
    }

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

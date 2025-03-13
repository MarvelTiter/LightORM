using LightORM.ExpressionSql;
using LightORM.Extension;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;

namespace LightORM.Cache;

internal class AbstractTableType
{
    public AbstractTableType(Type type)
    {
        Type = type;
    }

    public Type Type { get; }
}
internal static class TableContext
{
    internal static ITableContext? StaticContext { get; set; }
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
            var key = $"{type.FullName}_{col.PropertyName}_Setter";
            var reflectAction = StaticCache<Action<object, object?>>.GetOrAdd(key, () =>
            {
                var property = col.TableType.GetProperty(col.PropertyName)!;
                return col.TableType.GetPropertySetter(property);
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
            var key = $"{type.FullName}_{col.PropertyName}_Getter";
            var reflectAction = StaticCache<Func<object, object?>>.GetOrAdd(key, () =>
            {
                var property = col.TableType.GetProperty(col.PropertyName)!;
                return col.TableType.GetPropertyAccessor(property);
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
            realType = StaticCache<AbstractTableType>.GetOrAdd(cacheKey, () =>
            {
                var rt = StaticCache<TableEntity>.Values.Where(x => type.IsAssignableFrom(x.Type)).FirstOrDefault()?.Type;
                if (rt is null) LightOrmException.Throw("无法解析的表");
                return new AbstractTableType(rt!);
            }).Type;
            return GetTableInfo(realType);
        }

        if (realType.HasAttribute<LightFlatAttribute>())
        {

        }

        var entityInfoCache = StaticCache<TableEntity>.GetOrAdd(cacheKey, () =>
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

            var propertyColumnInfos = propertyInfos.SelectMany(p => ScanProperty(type, p));
            entityInfo.Columns = [.. propertyColumnInfos];
            if (entityInfo.IsAnonymousType)
            {
                entityInfo.Alias = $"t{StaticCache<TableEntity>.Count}";
            }
            else
            {
                entityInfo.Alias = $"r{StaticCache<TableEntity>.Count}";
            }
            return entityInfo;
        });
        // 拷贝
        return entityInfoCache with { };
    }

    private static IEnumerable<ITableColumnInfo> ScanProperty(Type table, PropertyInfo prop)
    {
        if (prop.HasAttribute<LightFlatAttribute>())
        {
            var flatProps = prop.PropertyType.GetProperties();
            foreach (var item in flatProps)
            {
                yield return new ColumnInfo(table, item, prop.PropertyType, false, true);
            }
            yield return new ColumnInfo(table, prop, prop.PropertyType, true, false);
        }
        else
        {
            yield return new ColumnInfo(table, prop, null, false, false);
        }
    }
}

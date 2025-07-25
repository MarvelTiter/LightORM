﻿using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace LightORM.Utils;
internal static class ColumnInfoExtensions
{
    public static ITableColumnInfo? GetColumn(this TableInfo ti, string columnName)
        => GetColumn(ti.TableEntityInfo, columnName);
    public static ITableColumnInfo? GetColumn(this ITableEntityInfo entityInfo, string columnName)
    {
        for (int i = 0; i < entityInfo.Columns.Length; i++)
        {
            if (entityInfo.Columns[i].PropertyName == columnName)
                return entityInfo.Columns[i];
        }
        return null;
    }
}
internal static class ObjectExtensions
{
    public static Func<object, object> GetPropertyAccessor(this Type type, PropertyInfo property)
    {
        /*
         * obj => (object)(((T)obj).Property)
         */
        var p = Expression.Parameter(typeof(object), "obj");
        var ins = Expression.Convert(p, type);
        var prop = Expression.Property(ins, property);
        var ret = Expression.Convert(prop, typeof(object));
        var lambda = Expression.Lambda<Func<object, object>>(ret, p);
        return lambda.Compile();
    }

    public static Action<object, object?> GetPropertySetter(this Type type, PropertyInfo property)
    {
        if (!property.CanWrite)
        {
            throw new LightOrmException($"{property.DeclaringType!.FullName}.{property.Name} is readonly");
        }
        var setMethod = property.GetSetMethod()!;
        var p = Expression.Parameter(typeof(object), "obj");
        var v = Expression.Parameter(typeof(object), "value");
        var ins = Expression.Convert(p, type);
        var typedV = Expression.Convert(v, property.PropertyType);
        var condition = Expression.NotEqual(v, Expression.Constant(null));
        var ifTrue = Expression.Call(ins, setMethod, typedV);
        var body = Expression.IfThen(condition, ifTrue);
        var lambda = Expression.Lambda<Action<object, object?>>(body, p, v);
        return lambda.Compile();
    }

    public static Func<object, object> GetFlatPropertyAccessor(this Type type, PropertyInfo property, PropertyInfo aggregate)
    {
        /*
         * obj => (object)(((T)obj).Aggregate.Property)
         */
        var p = Expression.Parameter(typeof(object), "obj");
        var ins = Expression.Convert(p, type);
        var agg = Expression.Property(ins, aggregate);
        var prop = Expression.Property(agg, property);
        var ret = Expression.Convert(prop, typeof(object));
        var lambda = Expression.Lambda<Func<object, object>>(ret, p);
        return lambda.Compile();
    }

    public static Action<object, object?> GetFlatPropertySetter(this Type type, PropertyInfo property, PropertyInfo aggregate)
    {
        /*
         * (obj, value) => (object)(((T)obj).Aggregate.Property = (MemberType)value)
         */
        if (!property.CanWrite)
        {
            throw new LightOrmException($"{property.DeclaringType!.FullName}.{property.Name} is readonly");
        }
        var setMethod = property.GetSetMethod()!;
        var p = Expression.Parameter(typeof(object), "obj");
        var v = Expression.Parameter(typeof(object), "value");
        var ins = Expression.Property(Expression.Convert(p, type), aggregate);
        var typedV = Expression.Convert(v, property.PropertyType);
        var condition = Expression.NotEqual(v, Expression.Constant(null));
        var ifTrue = Expression.Call(ins, setMethod, typedV);
        var body = Expression.IfThen(condition, ifTrue);
        var lambda = Expression.Lambda<Action<object, object?>>(body, p, v);
        return lambda.Compile();
    }

    //public static Dictionary<string, object> AccessObjectValues<T>(this T obj, IEnumerable<string> props)
    //{
    //    /*
    //     * var dictionary = new Dictionary<string, object>();
    //     * dictionary.Add(
    //     */
    //}
}

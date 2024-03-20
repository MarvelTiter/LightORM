using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace LightORM.Utils;

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

    //public static Dictionary<string, object> AccessObjectValues<T>(this T obj, IEnumerable<string> props)
    //{
    //    /*
    //     * var dictionary = new Dictionary<string, object>();
    //     * dictionary.Add(
    //     */
    //}
}

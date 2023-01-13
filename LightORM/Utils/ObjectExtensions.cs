using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace MDbContext.Utils
{
    internal static class ObjectExtensions
    {
        public static TValue AccessValue<TEntity, TValue>(this TEntity obj, string name)
        {
            Type type = typeof(TEntity);
            var p = Expression.Parameter(type, "p");
            var prop = type.GetProperty(name);
            if (prop == null)
                throw new ArgumentException($"No such Property {name} in Type {type.Name}");
            if (!prop.CanRead)
                throw new ArgumentException($"Property {name} in Type {type.Name} can't read");
            var getMethod = Expression.Call(p, prop.GetGetMethod(true));
            var lambda = Expression.Lambda<Func<TEntity, TValue>>(getMethod).Compile();
            return lambda(obj);
        }

        public static object AccessValue(this object obj, Type entityType, string name)
        {
            var p = Expression.Parameter(typeof(object), "p");
            var prop = entityType.GetProperty(name);
            if (prop == null)
                throw new ArgumentException($"No such Property {name} in Type {entityType.Name}");
            if (!prop.CanRead)
                throw new ArgumentException($"Property {name} in Type {entityType.Name} can't read");
            var getMethod = Expression.Call(Expression.Convert(p, entityType), prop.GetGetMethod(true));
            var converted = Expression.Convert(getMethod, typeof(object));
            var lambda = Expression.Lambda<Func<object, object>>(converted, p).Compile();
            return lambda(obj);
        }
    }
}

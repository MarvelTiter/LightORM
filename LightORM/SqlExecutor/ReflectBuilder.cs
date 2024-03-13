using LightORM.SqlExecutor.Service;
using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;
using System.Text;

namespace LightORM.SqlExecutor;
public class ReflectBuilder : IDeserializer
{
    public Func<IDataReader, object> BuildDeserializer<T>(IDataReader reader)
    {
        Type targetType = typeof(T);
        var props = targetType.GetProperties(BindingFlags.Public | BindingFlags.Instance);

        return r =>
        {
            var entity = Activator.CreateInstance(targetType);
            foreach (var t in props)
            {
                if (!t.CanWrite) continue;
                var index = r.GetOrdinal(t.Name);
                var value = r.GetValue(index);
#if NET40
                t.SetValue(entity, ChangeType(value, t.PropertyType), null);

#else
                t.SetValue(entity, ChangeType(value, t.PropertyType));
#endif
            }
            return entity;
        };
    }

    private static object ChangeType(object value, Type type)
    {
        if (value == null && type.IsGenericType) return Activator.CreateInstance(type);
        if (value == null || value == DBNull.Value) return default;
        if (type == value.GetType()) return value;
        if (type.IsEnum)
        {
            if (value is string)
                return Enum.Parse(type, value as string);
            else
                return Enum.ToObject(type, value);
        }
        if (!type.IsInterface && type.IsGenericType)
        {
            Type innerType = type.GetGenericArguments()[0];
            object innerValue = ChangeType(value, innerType);
            return Activator.CreateInstance(type, new object[] { innerValue });
        }
        if (value is string && type == typeof(Guid)) return new Guid(value as string);
        if (value is string && type == typeof(Version)) return new Version(value as string);
        if (!(value is IConvertible)) return value;
        return Convert.ChangeType(value, Nullable.GetUnderlyingType(type) ?? type);
    }
}

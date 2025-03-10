using LightORM.DbStruct;
using System.Linq;

namespace LightORM.Extension;

internal static class TypeExtension
{
    public static bool IsAnonymous(this Type? type)
    {
        return type?.FullName?.StartsWith("<>f__AnonymousType") == true;
    }

    public static bool IsFlat(this Type? type)
    {
        return type?.HasAttribute<LightFlatAttribute>() == true;
    }

    internal static DbTable CollectDbTableInfo(this Type tableType)
    {
        var tableName = tableType.GetAttribute<LightTableAttribute>()?.Name ?? tableType.Name;
        var columns = CollectColumns(tableType);
        var indexs = CollectIndexs(tableType, columns);
        return new DbTable { Name = tableName, Columns = columns, Indexs = indexs };
    }

    public static Type GetRealType(this Type type, out bool isCollection)
    {
        var t = type;
        isCollection = false;
        if (type.IsArray)
        {
            t = type.GetElementType()!;
            isCollection = true;
        }
        else if (type.IsGenericType)
        {
            t = type.GetGenericArguments()[0]!;
            isCollection = true;
        }
        return t;
    }

    public static bool IsNumber(this Type type)
    {
        type = Nullable.GetUnderlyingType(type) ?? type;
        return Type.GetTypeCode(type) switch
        {
            TypeCode.UInt16 or TypeCode.UInt32 or TypeCode.UInt64 or TypeCode.Int16 or TypeCode.Int32 or TypeCode.Int64 or TypeCode.Decimal or TypeCode.Double or TypeCode.Single => true,
            _ => false,
        };
    }

    public static bool IsBoolean(this Type type)
    {
        type = Nullable.GetUnderlyingType(type) ?? type;
        return Type.GetTypeCode(type) switch
        {
            TypeCode.Boolean => true,
            _ => false,
        };
    }

    private static List<DbIndex> CollectIndexs(Type tableType, List<DbColumn> columns)
    {
        IEnumerable<LightTableIndexAttribute> attrs = tableType.GetCustomAttributes(false).Where(a => a is LightTableIndexAttribute).Cast<LightTableIndexAttribute>();
        var indexs = new List<DbIndex>();
        foreach (LightTableIndexAttribute item in attrs)
        {
            indexs.Add(new()
            {
                Columns = item.Indexs.Select(p => columns.FirstOrDefault(c => c.PropName == p).Name) ?? Enumerable.Empty<string>(),
                DbIndexType = item.DbIndexType,
                Name = item.Name
            });
        }
        return indexs;
    }

    private static List<DbColumn> CollectColumns(Type tableType)
    {
        var props = tableType.GetProperties();
        var columns = new List<DbColumn>();
        foreach (var prop in props)
        {
            var ignore = prop.GetAttribute<IgnoreAttribute>();
            if (ignore != null) { continue; }
            var columnInfo = prop.GetAttribute<LightColumnAttribute>();
            columns.Add(new DbColumn
            {
                Name = columnInfo?.Name ?? prop.Name,
                PropName = prop.Name,
                PrimaryKey = columnInfo?.PrimaryKey ?? false,
                AutoIncrement = columnInfo?.AutoIncrement ?? false,
                NotNull = columnInfo?.NotNull ?? false,
                Length = columnInfo?.Length,
                Default = columnInfo?.Default,
                Comment = columnInfo?.Comment,
                DataType = prop.PropertyType,
            });
        }
        return columns;
    }

    private static readonly Dictionary<Type, object?> typeDefaultValueCache = new()
    {
        [typeof(object)] = default,
        [typeof(string)] = default(char),
        [typeof(bool)] = default(bool),
        [typeof(DateTime)] = default(DateTime),
        [typeof(Guid)] = default(Guid),
        [typeof(int)] = default(int),
        [typeof(uint)] = default(uint),
        [typeof(long)] = default(long),
        [typeof(ulong)] = default(ulong),
        [typeof(decimal)] = default(decimal),
        [typeof(double)] = default(double),
        [typeof(float)] = default(float),
        [typeof(short)] = default(short),
        [typeof(ushort)] = default(ushort),
        [typeof(byte)] = default(byte),
        [typeof(sbyte)] = default(sbyte),
        [typeof(char)] = default(char),
    };
    public static object? TypeDefaultValue(this Type type)
    {
        typeDefaultValueCache.TryGetValue(type, out var value);
        return value;
    }
}

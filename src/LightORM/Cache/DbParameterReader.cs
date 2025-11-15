using System.Collections;
using System.Collections.Concurrent;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Text.RegularExpressions;

namespace LightORM.Cache;

internal static class DbParameterReader
{
    private static readonly ConcurrentDictionary<string, Action<DbCommand, object>> cacheReaders = [];
    private static readonly ConcurrentDictionary<string, Func<object, Dictionary<string, object>>> readObjectToDicCache = [];
    public static Action<DbCommand, object> GetDbParameterReader(string connectionString, string commandText,
#if NET8_0_OR_GREATER
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
#endif
        Type paramaterType)
    {
        if (paramaterType == typeof(Dictionary<string, object>))
        {
            return ReadDictionary;
        }
        Certificate cer = new(connectionString, commandText, paramaterType);
        return cacheReaders.GetOrAdd($"DbParameterReader_{cer}", _ =>
        {
            return (cmd, obj) =>
            {
                var reader = CreateReader(cmd.CommandText, paramaterType);
                reader.Invoke(cmd, obj);
                SetDbType(cmd);
            };
        });
    }

    public static Dictionary<string, object> ReadToDictionary(string sql, object value)
    {
        if (value is null)
        {
            return [];
        }
        if (value is Dictionary<string, object> d)
        {
            return d;
        }
        var type = value.GetType();
        var key = $"{sql}_{type.FullName}";
        var func = readObjectToDicCache.GetOrAdd(key, _ =>
        {
            return CreateReadToDictionary(sql, type);
        });
        return func.Invoke(value);
    }

    private static void SetDbType(DbCommand cmd)
    {
        foreach (IDbDataParameter p in cmd.Parameters)
        {
            var dbType = GetDbType(p.Value!);
            if (dbType.HasValue)
                p.DbType = dbType.Value;
        }
    }

    private static void ReadDictionary(DbCommand cmd, object obj)
    {
        var dic = (Dictionary<string, object>)obj;
        foreach (var item in dic)
        {
            var p = cmd.CreateParameter(item.Key);
            var dbType = GetDbType(item.Value);
            if (dbType.HasValue)
                p.DbType = dbType.Value;
            p.Value = item.Value;
            cmd.Parameters.Add(p);
        }
    }
    static readonly MethodInfo createParameterMethodInfo = typeof(DbCommand).GetMethod("CreateParameter")!;
    static readonly MethodInfo listAddMethodInfo = typeof(IList).GetMethod("Add")!;
    static readonly MethodInfo dictionaryAdd = typeof(Dictionary<string, object>).GetMethod("Add")!;
    public static Action<DbCommand, object> CreateReader(string commandText,
#if NET8_0_OR_GREATER
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)]
#endif
        Type parameterType)
    {
        /*
         * (cmd, obj) => { 
         *    var p = cmd.CreateParameter();
         *    p.ParameterName = obj.XXX;
         *    p.Value = obj.XXX;
         *    cmd.Parameters.Add(p);
         * }
         */
        // (cmd, obj) => 
        PropertyInfo parameterCollection = typeof(DbCommand).GetProperty("Parameters")!;

        ParameterExpression cmdExp = Expression.Parameter(typeof(DbCommand), "cmd");
        ParameterExpression objExp = Expression.Parameter(typeof(object), "obj");
        var objType = parameterType;
        //TODO 优化
        var props = ExtractParameter(commandText, objType.GetProperties());
        // var temp
        var tempExp = Expression.Variable(typeof(IDataParameter), "temp");
        // var p = (Type)obj;
        var p1 = Expression.Variable(objType, "p");
        var paramExp = Expression.Assign(p1, Expression.Convert(objExp, objType));
        List<Expression> body = [
            tempExp,
            paramExp
        ];
        foreach (PropertyInfo prop in props)
        {
            // cmd.CreateParameter()
            var createParam = Expression.Call(cmdExp, createParameterMethodInfo);
            // temp = cmd.CreateParameter()                
            var pAssign = Expression.Assign(tempExp, createParam);
            var paramNameExp = Expression.Property(tempExp, "ParameterName");
            var valueExp = Expression.Property(tempExp, "Value");
            // temp.ParameterName = prop.Name
            var nameAssignExp = Expression.Assign(paramNameExp, Expression.Constant(prop.Name));
            // temp.Value = p.PropertyValue
            var valueAssignExp = Expression.Assign(valueExp, Expression.Convert(Expression.Property(paramExp, prop), typeof(object)));
            // cmd.Parameters.Add(temp)
            var addToList = Expression.Call(Expression.Property(cmdExp, parameterCollection), listAddMethodInfo, tempExp);

            body.Add(pAssign);
            body.Add(nameAssignExp);
            body.Add(valueAssignExp);
            body.Add(addToList);
        }
        var block = Expression.Block([tempExp, p1], body);
        var lambda = Expression.Lambda<Action<DbCommand, object>>(block, cmdExp, objExp);
        return lambda.Compile();
    }

    public static Func<object, Dictionary<string, object>> CreateReadToDictionary(string commandText,
#if NET8_0_OR_GREATER
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)]
#endif
        Type type)
    {
        var pExp = Expression.Parameter(typeof(object), "p");
        var retExp = Expression.Variable(typeof(Dictionary<string, object>), "ret");
        var realParam = Expression.Variable(type, "value");
        var assign = Expression.Assign(realParam, Expression.Convert(pExp, type));
        var retExpInited = Expression.Assign(retExp, Expression.New(typeof(Dictionary<string, object>)));
        List<Expression> blockBody = [
            assign,
            retExpInited,
            ];
        var all = type.GetProperties();
        var props = string.IsNullOrEmpty(commandText) ? all : ExtractParameter(commandText, all);
        foreach (var item in props)
        {
            var key = Expression.Constant(item.Name, typeof(string));
            var value = Expression.Convert(Expression.Property(realParam, item), typeof(object));
            var add = Expression.Call(retExp, dictionaryAdd, key, value);
            blockBody.Add(add);
        }
        blockBody.Add(retExp);
        var block = Expression.Block([realParam, retExp], blockBody);
        var lambda = Expression.Lambda<Func<object, Dictionary<string, object>>>(block, pExp);
        return lambda.Compile();
    }

    private static IEnumerable<PropertyInfo> ExtractParameter(string commandText, params PropertyInfo[] parameters)
    {
        foreach (PropertyInfo parameter in parameters)
        {
            if (Regex.IsMatch(commandText, "[?@:]" + parameter.Name + "([^\\p{L}\\p{N}_]+|$)", RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.CultureInvariant))
            {
                yield return parameter;
            }
        }
    }

    private static DbParameter CreateParameter(this DbCommand cmd, string parameterName)
    {
        if (cmd.Parameters.Contains(parameterName))
            return cmd.Parameters[parameterName];
        else
        {
            var p = cmd.CreateParameter();
            p.ParameterName = parameterName;
            return p;
        }
    }

    private static DbType? GetDbType(object value)
    {
        if (value == null)
        {
            return DbType.String;
        }
        var t = value.GetType();
        t = Nullable.GetUnderlyingType(t) ?? t;
        if (t.IsEnum)
        {
            t = Enum.GetUnderlyingType(t);
        }
        if (typeMapDbType.TryGetValue(t, out var v))
            return v;
        else return default;
    }

    readonly static Dictionary<Type, DbType> typeMapDbType = new Dictionary<Type, DbType>(37)
    {
        [typeof(byte)] = DbType.Byte,
        [typeof(sbyte)] = DbType.SByte,
        [typeof(short)] = DbType.Int16,
        [typeof(ushort)] = DbType.UInt16,
        [typeof(int)] = DbType.Int32,
        [typeof(uint)] = DbType.UInt32,
        [typeof(long)] = DbType.Int64,
        [typeof(ulong)] = DbType.UInt64,
        [typeof(float)] = DbType.Single,
        [typeof(double)] = DbType.Double,
        [typeof(decimal)] = DbType.Decimal,
        [typeof(bool)] = DbType.Boolean,
        [typeof(string)] = DbType.String,
        [typeof(char)] = DbType.StringFixedLength,
        [typeof(Guid)] = DbType.Guid,
        [typeof(DateTime)] = DbType.DateTime,
        [typeof(DateTimeOffset)] = DbType.DateTimeOffset,
        [typeof(TimeSpan)] = DbType.Time,
        [typeof(byte[])] = DbType.Binary,
        [typeof(byte?)] = DbType.Byte,
        [typeof(sbyte?)] = DbType.SByte,
        [typeof(short?)] = DbType.Int16,
        [typeof(ushort?)] = DbType.UInt16,
        [typeof(int?)] = DbType.Int32,
        [typeof(uint?)] = DbType.UInt32,
        [typeof(long?)] = DbType.Int64,
        [typeof(ulong?)] = DbType.UInt64,
        [typeof(float?)] = DbType.Single,
        [typeof(double?)] = DbType.Double,
        [typeof(decimal?)] = DbType.Decimal,
        [typeof(bool?)] = DbType.Boolean,
        [typeof(char?)] = DbType.StringFixedLength,
        [typeof(Guid?)] = DbType.Guid,
        [typeof(DateTime?)] = DbType.DateTime,
        [typeof(DateTimeOffset?)] = DbType.DateTimeOffset,
        [typeof(TimeSpan?)] = DbType.Time,
        [typeof(object)] = DbType.Object
    };
}

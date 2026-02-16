using System.Collections;
using System.Collections.Concurrent;
using System.Reflection;
using System.Text.RegularExpressions;

namespace LightORM.Cache;

internal static class DbParameterReader
{
    private static readonly ConcurrentDictionary<Certificate, Action<IDbCommand, object>> cacheReaders = [];
    private static readonly ConcurrentDictionary<Certificate, Func<object, Dictionary<string, object>>> readObjectToDicCache = [];
    public static Action<IDbCommand, object> GetDbParameterReader(string commandText
        , string prefix
        , Type paramaterType)
    {
        if (paramaterType == typeof(Dictionary<string, object>))
        {
            return ReadDictionary;
        }
        Certificate cer = new(commandText, prefix, paramaterType);
        return cacheReaders.GetOrAdd(cer, key => CreateReader(key.DbPrefix, key.Sql, key.ParameterType));
    }

    public static Dictionary<string, object> ObjectToDictionary(string prefix, string sql, object? value)
    {
        if (value is null)
        {
            return [];
        }
        if (value is Dictionary<string, object> d)
        {
            return d;
        }
        var cer = new Certificate(sql, prefix, value.GetType());
        var func = readObjectToDicCache.GetOrAdd(cer, key => CreateObjectToDictionary(key.DbPrefix, key.Sql, key.ParameterType));
        return func.Invoke(value);
    }

    private static void ReadDictionary(IDbCommand cmd, object obj)
    {
        var dic = (Dictionary<string, object>)obj;
        foreach (var item in dic)
        {
            if (item.Value is IDbDataParameter p)
            {
                cmd.Parameters.Add(p);
            }
            else
            {
                p = cmd.CreateParameter(item.Key);
                var value = item.Value;
                var (finalValue, dbType) = GetDbTypeAndValue(value);
                p.DbType = dbType;
                p.Value = finalValue;
                cmd.Parameters.Add(p);
            }
        }

        static (object? finalValue, DbType dbType) GetDbTypeAndValue(object? value)
        {
            if (value == null)
            {
                return (null, DbType.Object);
            }
            var t = value.GetType();
            var underlying = Nullable.GetUnderlyingType(t) ?? t;
            if (t.IsEnum)
            {
                var enumType = Enum.GetUnderlyingType(underlying);
                var converted = Convert.ChangeType(value, enumType);
                if (!typeMapDbType.TryGetValue(enumType, out var dbType))
                {
                    dbType = DbType.Object;
                }
                return (converted, dbType);
            }
            else
            {
                if (!typeMapDbType.TryGetValue(underlying, out var dbType))
                {
                    dbType = DbType.Object;
                }
                return (value, dbType);
            }
        }
    }
    static readonly MethodInfo createParameterMethodInfo = typeof(IDbCommand).GetMethod("CreateParameter")!;
    static readonly MethodInfo listAddMethodInfo = typeof(IList).GetMethod("Add")!;
    static readonly MethodInfo dictionaryAdd = typeof(Dictionary<string, object>).GetMethod("Add")!;
    public static Action<IDbCommand, object> CreateReader(string prefix, string commandText, Type parameterType)
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
        PropertyInfo parameterCollection = typeof(IDbCommand).GetProperty("Parameters")!;

        ParameterExpression cmdExp = Expression.Parameter(typeof(IDbCommand), "cmd");
        ParameterExpression objExp = Expression.Parameter(typeof(object), "obj");
        var objType = parameterType;
        //TODO 优化
        var props = ExtractParameter(prefix, commandText, objType.GetProperties());
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
            var (IsNullable, IsEnum) = GetUnderlyingType(prop.PropertyType, out var realType);

            // temp = cmd.CreateParameter()                
            var pAssign = Expression.Assign(
                tempExp,
                Expression.Call(cmdExp, createParameterMethodInfo));
            // temp.ParameterName = prop.Name
            var nameAssignExp = Expression.Assign(
                Expression.Property(tempExp, "ParameterName"),
                Expression.Constant(prop.Name, typeof(string)));

            var propAccess = Expression.Property(paramExp, prop);
            Expression finalValueExp;
            if (IsNullable && IsEnum)
            {
                // Nullable<Enum>：需要条件判断 null
                var isNull = Expression.Equal(propAccess, Expression.Constant(null, prop.PropertyType));
                finalValueExp = Expression.Condition(
                    isNull,
                    //Expression.Convert(Expression.Default(prop.PropertyType), typeof(object)),
                    Expression.Constant(null, typeof(object)),
                    Expression.Convert(Expression.Convert(propAccess, realType), typeof(object))
                );
            }
            else if (IsEnum)
            {
                var converted = Expression.Convert(propAccess, realType);
                finalValueExp = Expression.Convert(converted, typeof(object));
            }
            else
            {
                finalValueExp = Expression.Convert(propAccess, typeof(object));
            }

            var valueExp = Expression.Property(tempExp, "Value");

            // temp.Value = p.PropertyValue
            var valueAssignExp = Expression.Assign(
                Expression.Property(tempExp, "Value"),
                finalValueExp);
            if (!typeMapDbType.TryGetValue(realType, out var dbType))
            {
                dbType = DbType.Object;
            }

            //temp.DbType = dbType;
            var dbTypeAssignExp = Expression.Assign(
                Expression.Property(tempExp, "DbType"),
                Expression.Constant(dbType, typeof(DbType)));
            // cmd.Parameters.Add(temp)
            var addToList = Expression.Call(Expression.Property(cmdExp, parameterCollection), listAddMethodInfo, tempExp);

            body.Add(pAssign);
            body.Add(nameAssignExp);
            body.Add(valueAssignExp);
            body.Add(dbTypeAssignExp);
            body.Add(addToList);
        }
        var block = Expression.Block([tempExp, p1], body);
        var lambda = Expression.Lambda<Action<IDbCommand, object>>(block, cmdExp, objExp);
        return lambda.Compile();


        static (bool IsNullable, bool IsEnum) GetUnderlyingType(Type t, out Type realType)
        {
            var type = Nullable.GetUnderlyingType(t);
            var isNullable = false;
            var isEnum = false;
            if (type is not null)
            {
                isNullable = true;
            }
            type ??= t;
            if (type.IsEnum)
            {
                isEnum = true;
                type = Enum.GetUnderlyingType(type);
            }
            realType = type;
            return (isNullable, isEnum);
        }
    }

    public static Func<object, Dictionary<string, object>> CreateObjectToDictionary(string prefix, string commandText, Type type)
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
        var props = string.IsNullOrEmpty(commandText) ? all : ExtractParameter(prefix, commandText, all);
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

    private static IEnumerable<PropertyInfo> ExtractParameter(string prefix, string commandText, params PropertyInfo[] parameters)
    {
        foreach (PropertyInfo parameter in parameters)
        {
            // [^\\p{{L}}\\p{{N}}_]+|$ => \\b
            if (Regex.IsMatch(commandText, $"{Regex.Escape(prefix)}{Regex.Escape(parameter.Name)}(\\b)", RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.CultureInvariant))
            {
                yield return parameter;
            }
        }
    }

    private static IDbDataParameter CreateParameter(this IDbCommand cmd, string parameterName)
    {
        if (cmd.Parameters.Contains(parameterName))
            return (IDbDataParameter)cmd.Parameters[parameterName];
        else
        {
            var p = cmd.CreateParameter();
            p.ParameterName = parameterName;
            return p;
        }
    }

    readonly static Dictionary<Type, DbType> typeMapDbType = new(20)
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
        //[typeof(byte?)] = DbType.Byte,
        //[typeof(sbyte?)] = DbType.SByte,
        //[typeof(short?)] = DbType.Int16,
        //[typeof(ushort?)] = DbType.UInt16,
        //[typeof(int?)] = DbType.Int32,
        //[typeof(uint?)] = DbType.UInt32,
        //[typeof(long?)] = DbType.Int64,
        //[typeof(ulong?)] = DbType.UInt64,
        //[typeof(float?)] = DbType.Single,
        //[typeof(double?)] = DbType.Double,
        //[typeof(decimal?)] = DbType.Decimal,
        //[typeof(bool?)] = DbType.Boolean,
        //[typeof(char?)] = DbType.StringFixedLength,
        //[typeof(Guid?)] = DbType.Guid,
        //[typeof(DateTime?)] = DbType.DateTime,
        //[typeof(DateTimeOffset?)] = DbType.DateTimeOffset,
        //[typeof(TimeSpan?)] = DbType.Time,
        [typeof(object)] = DbType.Object
    };
}

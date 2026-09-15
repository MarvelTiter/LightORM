using LightORM.Extension;
using System.Collections;
using System.Collections.Concurrent;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Text.RegularExpressions;

namespace LightORM.Cache;

internal static class DbParameterReader
{
    private readonly record struct DbTypeInfo(object? FinalValue, DbType Type);
    private readonly record struct UnderlyingTypeInfo(bool IsNullable, bool IsEnum);

    private static readonly ConcurrentDictionary<Certificate, Action<IDbCommand, object, IDatabaseAdapter>> cacheReaders = [];
    private static readonly ConcurrentDictionary<Certificate, Action<object, Dictionary<string, DbParameterValue>>> readObjectToDicCache = [];
    public static void HandleDbParameter(this SqlExecuteContext context, IDatabaseAdapter adapter, DbCommand command)
    {
        if (string.IsNullOrWhiteSpace(context.Sql)) return;
        command.CommandText = context.Sql;
        command.CommandType = context.CommandType;
        var dbParameters = context.Parameter;
        if (dbParameters is not null && dbParameters is not NullDbParameter)
        {
            switch (dbParameters)
            {
                case Dictionary<string, DbParameterValue> valueDic:
                    // 结构化参数: 携带列元数据, 供数据库方言做类型化
                    BindDictionary(command, adapter, valueDic);
                    break;
                case Dictionary<string, object> objDic:
                    // 裸参数字典(无列信息): 按 CLR 默认推断
                    BindDictionary(command, adapter, objDic);
                    break;
                default:
                    // 对象/实体参数路径
                    var action = GetDbParameterReader(context.Sql!, adapter.Prefix, context.ParameterType);
                    action?.Invoke(command, dbParameters, adapter);
                    break;
            }
        }
    }

    public static Action<IDbCommand, object, IDatabaseAdapter> GetDbParameterReader(string commandText, string prefix,
#if NET8_0_OR_GREATER
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)]
#endif
        Type paramaterType)
    {
        Certificate cer = new(commandText, prefix, paramaterType);
        return cacheReaders.GetOrAdd(cer, key => CreateReader(key.DbPrefix, key.Sql, key.ParameterType));
    }

    public static void MergeObjectToDictionary<
#if NET8_0_OR_GREATER
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)]
#endif
    T>(string prefix, string sql, T? value, Dictionary<string, DbParameterValue> dic)
    {
        if (value is null)
        {
            return;
        }
        if (value is Dictionary<string, object> d)
        {
            foreach (var item in d)
            {
                if (dic.ContainsKey(item.Key))
                {
                    throw new LightOrmException($"查询参数：{item.Key} 重复");
                }
                dic.Add(item.Key, new DbParameterValue(null, item.Value));
            }
            return;
        }
        var cer = new Certificate(sql, prefix, typeof(T));
        var func = readObjectToDicCache.GetOrAdd(cer, key => CreateObjectToDictionary(key.DbPrefix, key.Sql, key.ParameterType));
        func.Invoke(value, dic);
    }

    private static void BindDictionary(IDbCommand cmd, IDatabaseAdapter adapter, Dictionary<string, DbParameterValue> dic)
    {
        foreach (var item in dic)
        {
            var p = CreateParameterValue(cmd, adapter, item.Key, item.Value.Column, item.Value.Value);
            AddParameterOnce(cmd, p);
        }
    }

    private static void BindDictionary(IDbCommand cmd, IDatabaseAdapter adapter, Dictionary<string, object> dic)
    {
        foreach (var item in dic)
        {
            var p = CreateParameterValue(cmd, adapter, item.Key, null, item.Value);
            AddParameterOnce(cmd, p);
        }
    }

    /// <summary>
    /// 将参数加入命令集合; 同名参数已存在时不再重复添加。
    /// </summary>
    private static void AddParameterOnce(IDbCommand cmd, IDbDataParameter parameter)
    {
        if (string.IsNullOrEmpty(parameter.ParameterName))
        {
            cmd.Parameters.Add(parameter);
            return;
        }
        if (!cmd.Parameters.Contains(parameter.ParameterName))
        {
            cmd.Parameters.Add(parameter);
        }
    }

    /// <summary>
    /// 统一的建参入口: 字典路径与对象(CreateReader)路径都经由本方法。
    /// 只负责创建/返回参数(不负责加入集合), 加入集合由调用方统一完成。
    /// 先交给数据库方言(<see cref="IDatabaseParameterBinder"/>)处理; 方言未接管时回退到 CLR 默认推断(原逻辑兜底)。
    /// </summary>
    private static IDbDataParameter CreateParameterValue(IDbCommand cmd, IDatabaseAdapter adapter, string name, ITableColumnInfo? column, object? value)
    {
        if (value is IDbDataParameter exitingParameter)
        {
            if (string.IsNullOrEmpty(exitingParameter.ParameterName))
            {
                exitingParameter.ParameterName = name;
            }
            return exitingParameter;
        }
        var p = cmd.CreateParameter();
        p.ParameterName = name;
        if (adapter is IDatabaseParameterBinder binder && binder.BindParameter(p, column, value))
        {
            return p;
        }
        var (finalValue, dbType) = GetDbTypeAndValue(value);
        p.DbType = dbType;
        p.Value = finalValue;
        return p;
    }

    static DbTypeInfo GetDbTypeAndValue(object? value)
    {
        if (value == null)
        {
            return new(null, DbType.Object);
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
            return new(converted, dbType);
        }
        else
        {
            if (!typeMapDbType.TryGetValue(underlying, out var dbType))
            {
                dbType = DbType.Object;
            }
            return new(value, dbType);
        }
    }
    static readonly MethodInfo listAddMethodInfo = typeof(IList).GetMethod("Add")!;
    static readonly MethodInfo createParameterValueMethod = typeof(DbParameterReader).GetMethod(nameof(CreateParameterValue), BindingFlags.NonPublic | BindingFlags.Static)!;
    static readonly MethodInfo dictionaryAdd = typeof(DbParameterReader).GetMethod(nameof(TryAdd), BindingFlags.NonPublic | BindingFlags.Static)!;

    static void TryAdd(Dictionary<string, DbParameterValue> dic, string key, object value)
    {
        dic.TryAdd(key, new DbParameterValue(null, value));
    }

#if NET8_0_OR_GREATER
    [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(IDataParameter))]
    [UnconditionalSuppressMessage("Trimming", "IL2026", Justification = "已通过DynamicDependency配置")]
#endif
    public static Action<IDbCommand, object, IDatabaseAdapter> CreateReader(string prefix, string commandText,
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
        // (cmd, obj, adapter) => 
        PropertyInfo parameterCollection = typeof(IDbCommand).GetProperty("Parameters")!;

        ParameterExpression cmdExp = Expression.Parameter(typeof(IDbCommand), "cmd");
        ParameterExpression objExp = Expression.Parameter(typeof(object), "obj");
        ParameterExpression adapterExp = Expression.Parameter(typeof(IDatabaseAdapter), "adapter");
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
            // temp = DbParameterReader.CreateParameterValue(cmd, adapter, "propName", null, (object)value)
            var (IsNullable, IsEnum) = GetUnderlyingType(prop.PropertyType, out var realType);

            var propAccess = Expression.Property(paramExp, prop);
            Expression finalValueExp;
            if (IsNullable && IsEnum)
            {
                // Nullable<Enum>：需要条件判断 null
                var isNull = Expression.Equal(propAccess, Expression.Constant(null, prop.PropertyType));
                finalValueExp = Expression.Condition(
                    isNull,
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

            var callHelper = Expression.Convert(
                Expression.Call(createParameterValueMethod,
                    cmdExp,
                    adapterExp,
                    Expression.Constant(prop.Name, typeof(string)),
                    Expression.Constant(null, typeof(ITableColumnInfo)),
                    finalValueExp),
                typeof(IDataParameter));
            var pAssign = Expression.Assign(tempExp, callHelper);

            // cmd.Parameters.Add(temp)
            var addToList = Expression.Call(Expression.Property(cmdExp, parameterCollection), listAddMethodInfo, tempExp);

            body.Add(pAssign);
            body.Add(addToList);
        }
        var block = Expression.Block([tempExp, p1], body);
        var lambda = Expression.Lambda<Action<IDbCommand, object, IDatabaseAdapter>>(block, cmdExp, objExp, adapterExp);
        return lambda.Compile();


        static UnderlyingTypeInfo GetUnderlyingType(Type t, out Type realType)
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
            return new(isNullable, isEnum);
        }
    }

    public static Action<object, Dictionary<string, DbParameterValue>> CreateObjectToDictionary(string prefix, string commandText,
#if NET8_0_OR_GREATER
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)]
#endif
        Type type)
    {
        var pExp = Expression.Parameter(typeof(object), "p");
        var dicExp = Expression.Parameter(typeof(Dictionary<string, DbParameterValue>), "dic");
        var realParam = Expression.Variable(type, "value");
        var assign = Expression.Assign(realParam, Expression.Convert(pExp, type));
        List<Expression> blockBody = [
            assign
            ];
        var all = type.GetProperties();
        var props = string.IsNullOrEmpty(commandText) ? all : ExtractParameter(prefix, commandText, all);
        foreach (var item in props)
        {
            var key = Expression.Constant(item.Name, typeof(string));
            var value = Expression.Convert(Expression.Property(realParam, item), typeof(object));
            var add = Expression.Call(dictionaryAdd, dicExp, key, value);
            blockBody.Add(add);
        }
        var block = Expression.Block([realParam], blockBody);
        var lambda = Expression.Lambda<Action<object, Dictionary<string, DbParameterValue>>>(block, pExp, dicExp);
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

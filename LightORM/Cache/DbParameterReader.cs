using System.Collections;
using System.Data;
using System.Reflection;
using System.Text.RegularExpressions;

namespace LightORM.Cache;

internal static class DbParameterReader
{
    public static Action<IDbCommand, object?>? GetDbParameterReader(string commandText, Type paramaterType)
    {
        Models.Certificate cer = new Models.Certificate(commandText, paramaterType);
        return StaticCache<Action<IDbCommand, object?>>.GetOrAdd($"DbParameterReader_{cer}", () =>
        {
            return (cmd, obj) =>
            {
                if (obj is Dictionary<string, object>)
                {
                    ReadDictionary(cmd, obj);
                }
                else
                {
                    var reader = CreateReader(commandText, paramaterType);
                    reader?.Invoke(cmd, obj!);
                    SetDbType(cmd);
                }
            };
        });
    }

    private static void SetDbType(IDbCommand cmd)
    {
        foreach (IDbDataParameter p in cmd.Parameters)
        {
            var dbType = GetDbType(p.Value);
            if (dbType.HasValue)
                p.DbType = dbType.Value;
        }
    }

    private static void ReadDictionary(IDbCommand cmd, object obj)
    {
        var dic = (Dictionary<string, object>)obj!;
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

    private static Action<IDbCommand, object> CreateReader(string commandText, Type parameterType)
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
        MethodInfo createParameterMethodInfo = typeof(IDbCommand).GetMethod("CreateParameter")!;
        PropertyInfo parameterCollection = typeof(IDbCommand).GetProperty("Parameters")!;
        MethodInfo listAddMethodInfo = typeof(IList).GetMethod("Add")!;

        ParameterExpression cmdExp = Expression.Parameter(typeof(IDbCommand), "cmd");
        ParameterExpression objExp = Expression.Parameter(typeof(object), "obj");
        var objType = parameterType;
        //TODO 优化
        var props = ExtractParameter(commandText, objType.GetProperties());
        // var temp
        var tempExp = Expression.Variable(typeof(IDataParameter), "temp");
        // var p = (Type)obj;
        var p1 = Expression.Variable(objType, "p");
        var paramExp = Expression.Assign(p1, Expression.Convert(objExp, objType));
        List<Expression> body = new List<Expression>
        {
            tempExp,
            paramExp
        };
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
        var block = Expression.Block(new[] { tempExp, p1 }, body);
        var lambda = Expression.Lambda<Action<IDbCommand, object>>(block, cmdExp, objExp);
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

    private static DbType? GetDbType(object value)
    {
        var t = value.GetType();
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

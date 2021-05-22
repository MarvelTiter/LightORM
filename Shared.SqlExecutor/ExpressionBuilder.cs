using MDbContext.SqlExecutor.Service;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MDbContext.SqlExecutor {
    public class ExpressionBuilder : IDeserializer {

        private static readonly MethodInfo DataRecord_ItemGetter_Int = typeof(IDataRecord).GetMethod("get_Item", new Type[] { typeof(int) });
        private static readonly MethodInfo DataRecord_GetOrdinal = typeof(IDataRecord).GetMethod("GetOrdinal");
        private static readonly MethodInfo DataReader_Read = typeof(IDataReader).GetMethod("Read");
        private static readonly MethodInfo Convert_ChangeType = typeof(ExpressionBuilder).GetMethod("ChangeType", BindingFlags.NonPublic | BindingFlags.Static);
        private static readonly MethodInfo DataRecord_IsDBNull = typeof(IDataRecord).GetMethod("IsDBNull");
        private static readonly MethodInfo DataRecord_GetValue = typeof(IDataRecord).GetMethod("GetValue");
        private static readonly MethodInfo DataRecord_GetFieldType = typeof(IDataRecord).GetMethod("GetFieldType");

        private static readonly MethodInfo DataRecord_GetByte = typeof(IDataRecord).GetMethod("GetByte");
        private static readonly MethodInfo DataRecord_GetInt16 = typeof(IDataRecord).GetMethod("GetInt16");
        private static readonly MethodInfo DataRecord_GetInt32 = typeof(IDataRecord).GetMethod("GetInt32");
        private static readonly MethodInfo DataRecord_GetInt64 = typeof(IDataRecord).GetMethod("GetInt64");
        private static readonly MethodInfo DataRecord_GetFloat = typeof(IDataRecord).GetMethod("GetFloat");
        private static readonly MethodInfo DataRecord_GetDouble = typeof(IDataRecord).GetMethod("GetDouble");
        private static readonly MethodInfo DataRecord_GetDecimal = typeof(IDataRecord).GetMethod("GetDecimal");
        private static readonly MethodInfo DataRecord_GetBoolean = typeof(IDataRecord).GetMethod("GetBoolean");
        private static readonly MethodInfo DataRecord_GetString = typeof(IDataRecord).GetMethod("GetString");
        private static readonly MethodInfo DataRecord_GetChar = typeof(IDataRecord).GetMethod("GetChar");
        private static readonly MethodInfo DataRecord_GetGuid = typeof(IDataRecord).GetMethod("GetGuid");
        private static readonly MethodInfo DataRecord_GetDateTime = typeof(IDataRecord).GetMethod("GetDateTime");

        readonly static Dictionary<Type, MethodInfo> typeMapMethod = new Dictionary<Type, MethodInfo>(37) {
            [typeof(byte)] = DataRecord_GetByte,
            [typeof(sbyte)] = DataRecord_GetByte,
            [typeof(short)] = DataRecord_GetInt16,
            [typeof(ushort)] = DataRecord_GetInt16,
            [typeof(int)] = DataRecord_GetInt32,
            [typeof(uint)] = DataRecord_GetInt32,
            [typeof(long)] = DataRecord_GetInt64,
            [typeof(ulong)] = DataRecord_GetInt64,
            [typeof(float)] = DataRecord_GetFloat,
            [typeof(double)] = DataRecord_GetDouble,
            [typeof(decimal)] = DataRecord_GetDecimal,
            [typeof(bool)] = DataRecord_GetBoolean,
            [typeof(string)] = DataRecord_GetString,
            [typeof(char)] = DataRecord_GetChar,
            [typeof(Guid)] = DataRecord_GetGuid,
            [typeof(DateTime)] = DataRecord_GetDateTime,
            //[typeof(byte[])] = DbType.Binary,
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
            //[typeof(object)] = DbType.Object
        };

        private static readonly Dictionary<Type, MethodInfo> typeMethodMap = new Dictionary<Type, MethodInfo>() {
            [typeof(string)] = DataRecord_GetString,
            [typeof(DateTime)] = DataRecord_GetDateTime,
            [typeof(decimal)] = DataRecord_GetDecimal,
            [typeof(float)] = DataRecord_GetFloat,
            [typeof(double)] = DataRecord_GetDouble,
            [typeof(short)] = DataRecord_GetInt16,
            [typeof(int)] = DataRecord_GetInt32,
            [typeof(long)] = DataRecord_GetInt64,
            [typeof(DateTime?)] = DataRecord_GetDateTime,
            [typeof(decimal?)] = DataRecord_GetDecimal,
            [typeof(float?)] = DataRecord_GetFloat,
            [typeof(double?)] = DataRecord_GetDouble,
            [typeof(short?)] = DataRecord_GetInt16,
            [typeof(int?)] = DataRecord_GetInt32,
            [typeof(long?)] = DataRecord_GetInt64,
        };
        public Func<IDataReader, object> BuildDeserializer(IDataReader reader, Type targetType) {
            IDataReader dataReader = reader;
            Type type = targetType;

            //List<MemberBinding> Bindings = new List<MemberBinding>();
            Type SourceType = typeof(IDataReader);
            ParameterExpression SourceInstance = Expression.Parameter(SourceType, "reader");

            DataTable SchemaTable = dataReader.GetSchemaTable();

            var props = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);

            #region memberBinding
            //List<MemberBinding> bindings = new List<MemberBinding>();
            //for (int i = 0; i < dataReader.FieldCount; i++) {
            //    var prop = props.FirstOrDefault(p => p.CanWrite && p.Name.ToLower() == dataReader.GetName(i).ToLower());
            //    if (prop == null) continue;
            //    var propType = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;
            //    var fieldType = dataReader.GetFieldType(i);

            //    //if (!typeMethodMap.TryGetValue(fieldType, out var mi))
            //    //    mi = DataRecord_GetValue;

            //    var indexExp = Expression.Constant(i);
            //    // reader.Get_XXX(i)
            //    var valueExp = Expression.Call(SourceInstance, DataRecord_GetValue, indexExp);
            //    //var valueConvertExp = Expression.Convert(valueExp, fieldType);
            //    var converted = GetRealValueExpression(valueExp, propType);
            //    var binding = Expression.Bind(prop, converted);
            //    bindings.Add(binding);
            //}
            //var exEntityInstance = Expression.New(targetType);
            //var bindExp = Expression.MemberInit(exEntityInstance, bindings.ToArray());
            //var lambda = Expression.Lambda<Func<IDataReader, object>>(bindExp, SourceInstance);
            #endregion

            List<Expression> body = new List<Expression>();
            var eExp = Expression.Variable(type, "e");
            var eAssignExp = Expression.Assign(eExp, Expression.New(type));
            // var e = new T();
            body.Add(eAssignExp);

            for (int i = 0; i < props.Length; i++) {
                var prop = props[i];
                if (prop == null || !prop.CanWrite) continue;
                var propType = prop.PropertyType;//Nullable.GetUnderlyingType(prop.PropertyType) ??
                var indexExp = Expression.Call(SourceInstance, DataRecord_GetOrdinal, Expression.Constant(prop.Name));
                Debug.WriteLine($"{prop.Name} => Index:{reader.GetOrdinal(prop.Name)}");

                //if (!typeMethodMap.TryGetValue(fieldType, out var mi))
                //    mi = DataRecord_GetValue;
                // e.XXX = reader.GetXX(i)
                var valueExp = Expression.Call(SourceInstance, DataRecord_GetValue, indexExp);
                var propAssign = Expression.Assign(Expression.Property(eExp, prop), GetRealValueExpression(valueExp, propType));
                //var isDBNullExp = Expression.Call(SourceInstance, DataRecord_IsDBNull, indexExp);

                //Expression.IfThenElse()

                body.Add(propAssign);
            }

            // return e;
            body.Add(eExp);
            var block = Expression.Block(
                new[] { eExp },
                body.ToArray()
                );
            var lambda = Expression.Lambda<Func<IDataReader, object>>(block, SourceInstance);

            return lambda.Compile();


            //
            Expression GetRealValueExpression(Expression valueExp, Type targetPropType) {

                //return Expression.Convert(Expression.Call(Convert_ChangeType, valueExp, Expression.Constant(targetPropType)), targetPropType);
                var temp = Expression.Variable(targetPropType, "temp");
                var checkDbNull = Expression.TypeIs(valueExp, typeof(DBNull));
                var checkNull = Expression.Equal(valueExp, Expression.Constant(null));
                valueExp = Expression.Convert(valueExp, typeof(object));
                /*
                 * if(reader.Get_XXX(i) is DBNull){
                 *     return default;
                 * } else {
                 *     return (type)Convert.ChangeType(reader.Get_XXX(i),type)
                 * }
                 */
                return Expression.Block(
                    new[] { temp },
                    Expression.IfThenElse(
                    Expression.OrElse(checkDbNull, checkNull),
                    Expression.Default(targetPropType),
                    Expression.Assign(temp, Expression.Convert(Expression.Call(Convert_ChangeType, valueExp, Expression.Constant(targetPropType)), targetPropType))),
                    temp
                    );
            }
        }

        private static object ChangeType(object value, Type type) {
            if (value == null && type.IsGenericType) return Activator.CreateInstance(type);
            if (value == null || value == DBNull.Value) return default;
            if (type == value.GetType()) return value;
            if (type.IsEnum) {
                if (value is string)
                    return Enum.Parse(type, value as string);
                else
                    return Enum.ToObject(type, value);
            }
            if (!type.IsInterface && type.IsGenericType) {
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
}

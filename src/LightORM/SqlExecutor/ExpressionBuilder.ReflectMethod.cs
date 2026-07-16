using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;

namespace LightORM.SqlExecutor;

internal partial class ExpressionBuilder
{
    private static readonly MethodInfo enumParseMethod = typeof(Enum).GetMethod("Parse", [typeof(Type), typeof(string), typeof(bool)])!;

    private static readonly MethodInfo DataRecord_GetByte
        = typeof(IDataRecord).GetMethod(nameof(IDataRecord.GetByte), [typeof(int)])!;
    private static readonly MethodInfo DataRecord_GetInt16
        = typeof(IDataRecord).GetMethod(nameof(IDataRecord.GetInt16), [typeof(int)])!;
    private static readonly MethodInfo DataRecord_GetInt32
        = typeof(IDataRecord).GetMethod(nameof(IDataRecord.GetInt32), [typeof(int)])!;
    private static readonly MethodInfo DataRecord_GetInt64
        = typeof(IDataRecord).GetMethod(nameof(IDataRecord.GetInt64), [typeof(int)])!;
    private static readonly MethodInfo DataRecord_GetFloat
        = typeof(IDataRecord).GetMethod(nameof(IDataRecord.GetFloat), [typeof(int)])!;
    private static readonly MethodInfo DataRecord_GetDouble
        = typeof(IDataRecord).GetMethod(nameof(IDataRecord.GetDouble), [typeof(int)])!;
    private static readonly MethodInfo DataRecord_GetDecimal
        = typeof(IDataRecord).GetMethod(nameof(IDataRecord.GetDecimal), [typeof(int)])!;
    private static readonly MethodInfo DataRecord_GetBoolean
        = typeof(IDataRecord).GetMethod(nameof(IDataRecord.GetBoolean), [typeof(int)])!;
    private static readonly MethodInfo DataRecord_GetString
        = typeof(IDataRecord).GetMethod(nameof(IDataRecord.GetString), [typeof(int)])!;
    private static readonly MethodInfo DataRecord_GetChar
        = typeof(IDataRecord).GetMethod(nameof(IDataRecord.GetChar), [typeof(int)])!;
    private static readonly MethodInfo DataRecord_GetGuid
        = typeof(IDataRecord).GetMethod(nameof(IDataRecord.GetGuid), [typeof(int)])!;
    private static readonly MethodInfo DataRecord_GetDateTime
        = typeof(IDataRecord).GetMethod(nameof(IDataRecord.GetDateTime), [typeof(int)])!;
    private static readonly MethodInfo DataRecord_IsDBNull
        = typeof(IDataRecord).GetMethod(nameof(IDataRecord.IsDBNull), [typeof(int)])!;
    private static readonly MethodInfo DataRecord_GetValue
        = typeof(IDataRecord).GetMethod(nameof(IDataRecord.GetValue), [typeof(int)])!;


    private static readonly MethodInfo DataRecord_GetUInt16 = typeof(ExpressionBuilder).GetMethod(nameof(RecordFieldToUInt16), BindingFlags.Public | BindingFlags.Static)!;
    private static readonly MethodInfo DataRecord_GetUInt = typeof(ExpressionBuilder).GetMethod(nameof(RecordFieldToUInt32), BindingFlags.Public | BindingFlags.Static)!;
    private static readonly MethodInfo DataRecord_GetUInt64 = typeof(ExpressionBuilder).GetMethod(nameof(RecordFieldToUInt64), BindingFlags.Public | BindingFlags.Static)!;
    private static readonly MethodInfo CustomStringParseToBoolean = typeof(ExpressionBuilder).GetMethod(nameof(CustomStringToBoolean), BindingFlags.Public | BindingFlags.Static)!;
    private static readonly MethodInfo Helper_GetBytes = typeof(ExpressionBuilder).GetMethod(nameof(RecordFieldToBytes), BindingFlags.Public | BindingFlags.Static)!;

    private static readonly MethodInfo StringDeserializer = typeof(ExpressionBuilder).GetMethod(nameof(RecordFieldStringDeserializer), BindingFlags.Public | BindingFlags.Static)!;
    private static readonly MethodInfo BytesDeserializer = typeof(ExpressionBuilder).GetMethod(nameof(RecordFieldBytesDeserializer), BindingFlags.Public | BindingFlags.Static)!;

    //private static readonly ConcurrentDictionary<(string, Type), MethodInfo> convertMethodInfos = [];
    //private static MethodInfo GetConvertMethod(string methodName, Type type)
    //{
    //    return convertMethodInfos.GetOrAdd((methodName, type), k =>
    //    {
    //        return typeof(Convert).GetMethod(k.Item1, [k.Item2])!;
    //    });
    //}

    private static readonly Dictionary<Type, MethodInfo> typeMapMethod = new(37)
    {
        [typeof(byte)] = DataRecord_GetByte,
        [typeof(sbyte)] = DataRecord_GetByte,
        [typeof(short)] = DataRecord_GetInt16,
        [typeof(ushort)] = DataRecord_GetUInt16,
        [typeof(int)] = DataRecord_GetInt32,
        [typeof(uint)] = DataRecord_GetUInt,
        [typeof(long)] = DataRecord_GetInt64,
        [typeof(ulong)] = DataRecord_GetUInt64,
        [typeof(float)] = DataRecord_GetFloat,
        [typeof(double)] = DataRecord_GetDouble,
        [typeof(decimal)] = DataRecord_GetDecimal,
        [typeof(bool)] = DataRecord_GetBoolean,
        [typeof(string)] = DataRecord_GetString,
        [typeof(char)] = DataRecord_GetChar,
        [typeof(Guid)] = DataRecord_GetGuid,
        [typeof(DateTime)] = DataRecord_GetDateTime,
        [typeof(byte[])] = Helper_GetBytes
    };

}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace LightORM.SqlExecutor;

internal partial class ExpressionBuilder
{
    private static readonly MethodInfo enumParseMethod = typeof(Enum).GetMethod("Parse", [typeof(Type), typeof(string), typeof(bool)])!;

    private static readonly MethodInfo DataRecord_GetByte = typeof(IDataRecord).GetMethod("GetByte", [typeof(int)])!;
    private static readonly MethodInfo DataRecord_GetInt16 = typeof(IDataRecord).GetMethod("GetInt16", [typeof(int)])!;
    private static readonly MethodInfo DataRecord_GetInt32 = typeof(IDataRecord).GetMethod("GetInt32", [typeof(int)])!;
    private static readonly MethodInfo DataRecord_GetInt64 = typeof(IDataRecord).GetMethod("GetInt64", [typeof(int)])!;
    private static readonly MethodInfo DataRecord_GetFloat = typeof(IDataRecord).GetMethod("GetFloat", [typeof(int)])!;
    private static readonly MethodInfo DataRecord_GetDouble = typeof(IDataRecord).GetMethod("GetDouble", [typeof(int)])!;
    private static readonly MethodInfo DataRecord_GetDecimal = typeof(IDataRecord).GetMethod("GetDecimal", [typeof(int)])!;
    private static readonly MethodInfo DataRecord_GetBoolean = typeof(IDataRecord).GetMethod("GetBoolean", [typeof(int)])!;
    private static readonly MethodInfo DataRecord_GetString = typeof(IDataRecord).GetMethod("GetString", [typeof(int)])!;
    private static readonly MethodInfo DataRecord_GetChar = typeof(IDataRecord).GetMethod("GetChar", [typeof(int)])!;
    private static readonly MethodInfo DataRecord_GetGuid = typeof(IDataRecord).GetMethod("GetGuid", [typeof(int)])!;
    private static readonly MethodInfo DataRecord_GetDateTime = typeof(IDataRecord).GetMethod("GetDateTime", [typeof(int)])!;

    private static readonly MethodInfo DataRecord_IsDBNull = typeof(IDataRecord).GetMethod("IsDBNull", [typeof(int)])!;
    private static readonly MethodInfo DataRecord_GetValue = typeof(IDataRecord).GetMethod("GetValue", [typeof(int)])!;


    private static readonly MethodInfo DataRecord_GetUInt16 = typeof(ExpressionBuilder).GetMethod(nameof(RecordFieldToUInt16), BindingFlags.Public | BindingFlags.Static)!;
    private static readonly MethodInfo DataRecord_GetUInt = typeof(ExpressionBuilder).GetMethod(nameof(RecordFieldToUInt32), BindingFlags.Public | BindingFlags.Static)!;
    private static readonly MethodInfo DataRecord_GetUInt64 = typeof(ExpressionBuilder).GetMethod(nameof(RecordFieldToUInt64), BindingFlags.Public | BindingFlags.Static)!;
    private static readonly MethodInfo CustomStringParseToBoolean = typeof(ExpressionBuilder).GetMethod(nameof(CustomStringToBoolean), BindingFlags.Public | BindingFlags.Static)!;
    private static readonly MethodInfo Helper_GetBytes = typeof(ExpressionBuilder).GetMethod(nameof(RecordFieldToBytes), BindingFlags.NonPublic | BindingFlags.Static)!;


    readonly static Dictionary<Type, MethodInfo> typeMapMethod = new(37)
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

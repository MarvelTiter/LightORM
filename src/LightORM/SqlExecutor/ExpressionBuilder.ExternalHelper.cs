using LightORM.Extension;
using System.Collections.Concurrent;

namespace LightORM.SqlExecutor;

internal partial class ExpressionBuilder
{
    static Type GetSafeIntermediateType(Type sourceType, Type targetType)
    {
        if (!sourceType.IsNumber() || !targetType.IsNumber())
        {
            return sourceType;
        }
        sourceType = GetUnderlyingType(sourceType).Type;
        targetType = GetUnderlyingType(targetType).Type;

        // 特殊处理 decimal
        if (sourceType == typeof(decimal) || targetType == typeof(decimal))
        {
            return HandleDecimalConversion(sourceType, targetType);
        }

        return sourceType;

        static Type HandleDecimalConversion(Type sourceType, Type targetType)
        {
            // decimal 到其他类型
            if (sourceType == typeof(decimal))
            {
                // decimal 可以安全转换为 double（因为 double 范围更大）
                if (targetType == typeof(double))
                    return typeof(double);

                // decimal 转换为 float 可能有精度损失，但不会溢出（float 范围比 decimal 大）
                if (targetType == typeof(float))
                    return typeof(float);

                // decimal 转换为整数类型可能溢出
                if (Type.GetTypeCode(targetType) == TypeCode.Int32)
                {
                    // 需要通过检查或使用 checked 转换
                    return typeof(decimal); // 保持 decimal，让后续转换处理
                }

                return typeof(decimal);
            }

            // 其他类型到 decimal
            if (targetType == typeof(decimal))
            {
                // double 到 decimal 可能溢出（因为 decimal 范围更小）
                if (sourceType == typeof(double))
                {
                    // double 的范围可能超出 decimal，需要特殊处理
                    return typeof(double); // 保持 double，避免精度损失
                }

                // float 到 decimal 通常安全（float 范围比 decimal 大，但需要检查具体值）
                if (sourceType == typeof(float))
                {
                    return typeof(float);
                }

                // 整数类型到 decimal 通常安全（decimal 范围足够大）
                if (Type.GetTypeCode(sourceType) == TypeCode.Int32)
                {
                    return typeof(decimal);
                }

                return typeof(decimal);
            }

            return typeof(decimal);
        }
    }

    private static ConcurrentDictionary<Type, byte> JsonMaps { get; } = [];

    public static void AddJsonTypeMap(Type type)
    {
        JsonMaps.TryAdd(type, 0);
    }

    public static bool ContainsJsonType(Type type) => JsonMaps.ContainsKey(type);

    public static bool CustomStringToBoolean(string valueString)
    {
        return ",是,1,Y,YES,TRUE,".Contains(valueString.ToUpper()) ? true : false;
    }

    public static byte[] RecordFieldToBytes(IDataRecord Reader, int Column)
    {
        long blobSize = Reader.GetBytes(Column, 0, null, 0, 0);
        if (blobSize > int.MaxValue)
            throw new ArgumentOutOfRangeException("MemoryStream cannot be larger than " + int.MaxValue);
        // 处理空数据
        if (blobSize == 0)
            return [];
        byte[] Buffer = new byte[blobSize];
        Reader.GetBytes(Column, 0, Buffer, 0, Buffer.Length);
        return Buffer;
    }

    public static uint RecordFieldToUInt32(IDataRecord Reader, int Column)
    {
        var value = Reader.GetInt32(Column);
        return value >= 0 ? (uint)value : throw new OverflowException("Negative value cannot be converted to uint");
    }

    public static ushort RecordFieldToUInt16(IDataRecord Reader, int Column)
    {
        var value = Reader.GetInt16(Column);
        return value >= 0 ? (ushort)value : throw new OverflowException("Negative value cannot be converted to ushort");
    }

    public static ulong RecordFieldToUInt64(IDataRecord Reader, int Column)
    {
        var value = Reader.GetInt64(Column);
        return value >= 0 ? (ulong)value : throw new OverflowException("Negative value cannot be converted to ulong");
    }

    public static object? RecordFieldStringDeserializer(string value, Type targetType)
    {
        var jsonHandler = ExpressionSqlOptions.Instance.Value.GetJsonHandler();
        return jsonHandler.Deserialize(value, targetType);
    }

    public static object? RecordFieldBytesDeserializer(byte[] value, Type targetType)
    {
        var jsonHandler = ExpressionSqlOptions.Instance.Value.GetJsonHandler();
        return jsonHandler.Deserialize(value, targetType);
    }
}
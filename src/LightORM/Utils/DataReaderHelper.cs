using System.Numerics;

namespace LightORM.Utils;

public static class DataReaderHelper
{
    public static byte? GetByteSafe(this IDataRecord record, int index)
    {
        if (index < 0 || index >= record.FieldCount || record.IsDBNull(index))
        {
            return default;
        }
        return record.GetByte(index);
    }

    public static short? GetInt16Safe(this IDataRecord record, int index)
    {
        if (index < 0 || index >= record.FieldCount || record.IsDBNull(index))
        {
            return default;
        }
        return record.GetInt16(index);
    }

    public static ushort? GetUInt16Safe(this IDataRecord record, int index)
    {
        if (index < 0 || index >= record.FieldCount || record.IsDBNull(index))
        {
            return default;
        }
        return ExpressionBuilder.RecordFieldToUInt16(record, index);
    }

    public static int? GetInt32Safe(this IDataRecord record, int index)
    {
        if (index < 0 || index >= record.FieldCount || record.IsDBNull(index))
        {
            return default;
        }
        return record.GetInt32(index);
    }

    public static uint? GetUInt32Safe(this IDataRecord record, int index)
    {
        if (index < 0 || index >= record.FieldCount || record.IsDBNull(index))
        {
            return default;
        }
        return ExpressionBuilder.RecordFieldToUInt32(record, index);
    }

    public static long? GetInt64Safe(this IDataRecord record, int index)
    {
        if (index < 0 || index >= record.FieldCount || record.IsDBNull(index))
        {
            return default;
        }
        return record.GetInt64(index);
    }

    public static ulong? GetUInt64Safe(this IDataRecord record, int index)
    {
        if (index < 0 || index >= record.FieldCount || record.IsDBNull(index))
        {
            return default;
        }
        return ExpressionBuilder.RecordFieldToUInt64(record, index);
    }

    public static float? GetFloatSafe(this IDataRecord record, int index)
    {
        if (index < 0 || index >= record.FieldCount || record.IsDBNull(index))
        {
            return default;
        }
        return record.GetFloat(index);
    }

    public static double? GetDoubleSafe(this IDataRecord record, int index)
    {
        if (index < 0 || index >= record.FieldCount || record.IsDBNull(index))
        {
            return default;
        }
        return record.GetDouble(index);
    }

    public static decimal? GetDecimalSafe(this IDataRecord record, int index)
    {
        if (index < 0 || index >= record.FieldCount || record.IsDBNull(index))
        {
            return default;
        }
        return record.GetDecimal(index);
    }

    public static bool? GetBooleanSafe(this IDataRecord record, int index)
    {
        if (index < 0 || index >= record.FieldCount || record.IsDBNull(index))
        {
            return default;
        }
        return record.GetBoolean(index);
    }

    public static string? GetStringSafe(this IDataRecord record, int index)
    {
        if (index < 0 || index >= record.FieldCount || record.IsDBNull(index))
        {
            return default;
        }
        return record.GetString(index);
    }

    public static char? GetCharSafe(this IDataRecord record, int index)
    {
        if (index < 0 || index >= record.FieldCount || record.IsDBNull(index))
        {
            return default;
        }
        return record.GetChar(index);
    }

    public static Guid? GetGuidSafe(this IDataRecord record, int index)
    {
        if (index < 0 || index >= record.FieldCount || record.IsDBNull(index))
        {
            return default;
        }
        return record.GetGuid(index);
    }

    public static DateTime? GetDateTimeSafe(this IDataRecord record, int index)
    {
        if (index < 0 || index >= record.FieldCount || record.IsDBNull(index))
        {
            return default;
        }
        return record.GetDateTime(index);
    }

    public static byte[]? GetBytesSafe(this IDataRecord record, int index)
    {
        if (index < 0 || index >= record.FieldCount || record.IsDBNull(index))
        {
            return default;
        }
        return ExpressionBuilder.RecordFieldToBytes(record, index);
    }

    //public static T HandleValueType<T>(object value, Type targetType)
    //{
    //    if (value.GetType() == targetType)
    //    {
    //        return (T)value!;
    //    }
    //}

    public static T GetValueOrDefault<T>(this IDataRecord record, int index)
    {
        if (index < 0 || index >= record.FieldCount)
            return default!;
        if (record.IsDBNull(index))
            return default!;
        return (T)record.GetValue(index);
    }
}

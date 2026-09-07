// =====================================================================
// 运行时支撑代码 —— 放在 MT.LightORM 主库中，生成器只负责"引用"它
// 目标框架：net462 / netstandard2.0 / net8.0 / net10.0，全部语法兼容
// 说明：本文件不是生成产物，生成器生成的代码依赖这里定义的类型。
// =====================================================================

using System;
using System.Collections.Concurrent;
using System.Data;
using System.Text;

namespace LightORM.Utils;

/// <summary>
/// 字段的 CLR 类型分类。生成代码用它做静态分派，替代 Expression.Convert。
/// </summary>
public enum FieldKind : byte
{
    Unknown = 0,
    Boolean,
    Byte,
    SByte,
    Int16,
    UInt16,
    Int32,
    UInt32,
    Int64,
    UInt64,
    Single,
    Double,
    Decimal,
    DateTime,
    DateTimeOffset,
    TimeSpan,
    Guid,
    String,
    Char,
    Bytes,
    Object,
}

public static class FieldKindResolver
{
    /// <summary>
    /// 把 reader.GetFieldType(ordinal) 归一化为 FieldKind。
    /// 只在"列绑定"阶段每个 ordinal 调用一次，不在热路径上。
    /// </summary>
    public static FieldKind Resolve(Type? type)
    {
        if (type is null)
        {
            return FieldKind.Unknown;
        }

        // 先处理非基元类型（GetTypeCode 对它们返回 Object）
        if (ReferenceEquals(type, typeof(byte[])))
        {
            return FieldKind.Bytes;
        }

        if (ReferenceEquals(type, typeof(Guid)))
        {
            return FieldKind.Guid;
        }

        if (ReferenceEquals(type, typeof(TimeSpan)))
        {
            return FieldKind.TimeSpan;
        }

        if (ReferenceEquals(type, typeof(DateTimeOffset)))
        {
            return FieldKind.DateTimeOffset;
        }

        switch (Type.GetTypeCode(type))
        {
            case TypeCode.Boolean: return FieldKind.Boolean;
            case TypeCode.Byte: return FieldKind.Byte;
            case TypeCode.SByte: return FieldKind.SByte;
            case TypeCode.Int16: return FieldKind.Int16;
            case TypeCode.UInt16: return FieldKind.UInt16;
            case TypeCode.Int32: return FieldKind.Int32;
            case TypeCode.UInt32: return FieldKind.UInt32;
            case TypeCode.Int64: return FieldKind.Int64;
            case TypeCode.UInt64: return FieldKind.UInt64;
            case TypeCode.Single: return FieldKind.Single;
            case TypeCode.Double: return FieldKind.Double;
            case TypeCode.Decimal: return FieldKind.Decimal;
            case TypeCode.DateTime: return FieldKind.DateTime;
            case TypeCode.String: return FieldKind.String;
            case TypeCode.Char: return FieldKind.Char;
            case TypeCode.Object: return FieldKind.Object;
            case TypeCode.DBNull: return FieldKind.Unknown;
            case TypeCode.Empty: return FieldKind.Unknown;
            default: return FieldKind.Unknown;
        }
    }
}

/// <summary>
/// 列绑定计划的缓存键计算。
/// 注意：这里刻意【不】使用 reader.GetSchemaTable()——
/// 很多 provider（SQLite、部分 Oracle/达梦驱动）的 GetSchemaTable 实现很慢甚至返回 null。
/// </summary>
public static class ReaderSchemaKey
{
    public static string Build(IDataReader reader)
    {
        var sb = new StringBuilder(128);
        int count = reader.FieldCount;
        sb.Append(count);
        for (int i = 0; i < count; i++)
        {
            sb.Append('|')
              .Append(reader.GetName(i))
              .Append('=')
              .Append((byte)FieldKindResolver.Resolve(reader.GetFieldType(i)));
        }

        return sb.ToString();
    }
}

/// <summary>
/// Compact 档使用的兼容读取器（见 Product.Deserializer.Compact.g.cs）。
/// Fast 档不调用这里，而是由生成器把 switch 直接展开到实体自己的方法里。
/// </summary>
public static class DataReaderReaders
{
    // ---------------- int / int? ----------------
    public static int ReadInt32(IDataReader r, int o, FieldKind k)
    {
        switch (k)
        {
            case FieldKind.Int32: return r.GetInt32(o);
            case FieldKind.Int64: return (int)r.GetInt64(o);
            case FieldKind.Int16: return r.GetInt16(o);
            case FieldKind.Byte: return r.GetByte(o);
            case FieldKind.SByte: return r.GetByte(o);
            case FieldKind.Decimal: return (int)r.GetDecimal(o);
            case FieldKind.Double: return (int)r.GetDouble(o);
            case FieldKind.Single: return (int)r.GetFloat(o);
            case FieldKind.Boolean: return r.GetBoolean(o) ? 1 : 0;
            case FieldKind.String: return int.Parse(r.GetString(o), System.Globalization.CultureInfo.CurrentCulture);
            default: return (int)Convert.ChangeType(r.GetValue(o), typeof(int), System.Globalization.CultureInfo.CurrentCulture);
        }
    }

    public static int? ReadInt32N(IDataReader r, int o, FieldKind k)
        => r.IsDBNull(o) ? (int?)null : ReadInt32(r, o, k);

    // ---------------- long / long? ----------------
    public static long ReadInt64(IDataReader r, int o, FieldKind k)
    {
        switch (k)
        {
            case FieldKind.Int64: return r.GetInt64(o);
            case FieldKind.Int32: return r.GetInt32(o);
            case FieldKind.Int16: return r.GetInt16(o);
            case FieldKind.Byte: return r.GetByte(o);
            case FieldKind.Decimal: return (long)r.GetDecimal(o);
            case FieldKind.Double: return (long)r.GetDouble(o);
            case FieldKind.Single: return (long)r.GetFloat(o);
            case FieldKind.String: return long.Parse(r.GetString(o), System.Globalization.CultureInfo.CurrentCulture);
            default: return (long)Convert.ChangeType(r.GetValue(o), typeof(long), System.Globalization.CultureInfo.CurrentCulture);
        }
    }

    public static long? ReadInt64N(IDataReader r, int o, FieldKind k)
        => r.IsDBNull(o) ? (long?)null : ReadInt64(r, o, k);

    // ---------------- string ----------------
    public static string? ReadString(IDataReader r, int o, FieldKind k)
    {
        switch (k)
        {
            case FieldKind.String: return r.GetString(o);
            case FieldKind.Char: return r.GetChar(o).ToString();
            case FieldKind.Guid: return r.GetGuid(o).ToString();
            case FieldKind.Bytes: return Encoding.UTF8.GetString(DataReaderConvert.ToBytes(r, o));
            default:
                var v = r.GetValue(o);
                return v is null or DBNull ? null : Convert.ToString(v, System.Globalization.CultureInfo.CurrentCulture);
        }
    }

    // ---------------- bool / bool? ----------------
    public static bool ReadBoolean(IDataReader r, int o, FieldKind k)
    {
        switch (k)
        {
            case FieldKind.Boolean: return r.GetBoolean(o);
            case FieldKind.Int32: return r.GetInt32(o) != 0;
            case FieldKind.Int64: return r.GetInt64(o) != 0;
            case FieldKind.Int16: return r.GetInt16(o) != 0;
            case FieldKind.Byte: return r.GetByte(o) != 0;
            case FieldKind.String: return DataReaderConvert.StringToBoolean(r.GetString(o));
            default: return Convert.ToBoolean(r.GetValue(o), System.Globalization.CultureInfo.CurrentCulture);
        }
    }

    public static bool? ReadBooleanN(IDataReader r, int o, FieldKind k)
        => r.IsDBNull(o) ? (bool?)null : ReadBoolean(r, o, k);

    // ---------------- DateTime / DateTime? ----------------
    public static DateTime ReadDateTime(IDataReader r, int o, FieldKind k)
    {
        switch (k)
        {
            case FieldKind.DateTime: return r.GetDateTime(o);
            case FieldKind.DateTimeOffset: return ((DateTimeOffset)r.GetValue(o)).DateTime;
            case FieldKind.String: return DateTime.Parse(r.GetString(o), System.Globalization.CultureInfo.CurrentCulture);
            case FieldKind.Int64: return new DateTime(r.GetInt64(o));
            default: return (DateTime)Convert.ChangeType(r.GetValue(o), typeof(DateTime), System.Globalization.CultureInfo.CurrentCulture);
        }
    }

    public static DateTime? ReadDateTimeN(IDataReader r, int o, FieldKind k)
        => r.IsDBNull(o) ? (DateTime?)null : ReadDateTime(r, o, k);

    // ---------------- decimal / decimal? ----------------
    public static decimal ReadDecimal(IDataReader r, int o, FieldKind k)
    {
        switch (k)
        {
            case FieldKind.Decimal: return r.GetDecimal(o);
            case FieldKind.Double: return (decimal)r.GetDouble(o);
            case FieldKind.Single: return (decimal)r.GetFloat(o);
            case FieldKind.Int64: return r.GetInt64(o);
            case FieldKind.Int32: return r.GetInt32(o);
            case FieldKind.Int16: return r.GetInt16(o);
            case FieldKind.Byte: return r.GetByte(o);
            case FieldKind.String: return decimal.Parse(r.GetString(o), System.Globalization.CultureInfo.CurrentCulture);
            default: return (decimal)Convert.ChangeType(r.GetValue(o), typeof(decimal), System.Globalization.CultureInfo.CurrentCulture);
        }
    }

    public static decimal? ReadDecimalN(IDataReader r, int o, FieldKind k)
        => r.IsDBNull(o) ? (decimal?)null : ReadDecimal(r, o, k);

    // ---------------- double / double? ----------------
    public static double ReadDouble(IDataReader r, int o, FieldKind k)
    {
        switch (k)
        {
            case FieldKind.Double: return r.GetDouble(o);
            case FieldKind.Single: return r.GetFloat(o);
            case FieldKind.Decimal: return (double)r.GetDecimal(o);
            case FieldKind.Int64: return r.GetInt64(o);
            case FieldKind.Int32: return r.GetInt32(o);
            case FieldKind.String: return double.Parse(r.GetString(o), System.Globalization.CultureInfo.CurrentCulture);
            default: return (double)Convert.ChangeType(r.GetValue(o), typeof(double), System.Globalization.CultureInfo.CurrentCulture);
        }
    }

    public static double? ReadDoubleN(IDataReader r, int o, FieldKind k)
        => r.IsDBNull(o) ? (double?)null : ReadDouble(r, o, k);

    // ---------------- Guid / Guid? ----------------
    public static Guid ReadGuid(IDataReader r, int o, FieldKind k)
    {
        switch (k)
        {
            case FieldKind.Guid: return r.GetGuid(o);
            case FieldKind.String: return Guid.Parse(r.GetString(o));
            case FieldKind.Bytes: return new Guid(DataReaderConvert.ToBytes(r, o));
            default: return (Guid)r.GetValue(o);
        }
    }

    public static Guid? ReadGuidN(IDataReader r, int o, FieldKind k)
        => r.IsDBNull(o) ? (Guid?)null : ReadGuid(r, o, k);
}

/// <summary>
/// 兜底/特殊转换。对应现有 ExpressionBuilder.ExternalHelper 里的那几个静态方法。
/// </summary>
public static class DataReaderConvert
{
    public static byte[] ToBytes(IDataRecord r, int o)
    {
        long size = r.GetBytes(o, 0, null, 0, 0);
        if (size > int.MaxValue)
        {
            throw new ArgumentOutOfRangeException(nameof(o), "MemoryStream cannot be larger than " + int.MaxValue);
        }

        if (size == 0)
        {
            return Array.Empty<byte>();
        }

        var buffer = new byte[size];
        r.GetBytes(o, 0, buffer, 0, buffer.Length);
        return buffer;
    }

    /// <summary>对应 ExpressionBuilder.CustomStringToBoolean</summary>
    public static bool StringToBoolean(string value)
        => ",是,1,Y,YES,TRUE,".Contains(value.ToUpperInvariant());

    public static ushort ToUInt16(IDataRecord r, int o)
    {
        var v = r.GetInt16(o);
        return v >= 0 ? (ushort)v : throw new OverflowException("Negative value cannot be converted to ushort");
    }

    public static uint ToUInt32(IDataRecord r, int o)
    {
        var v = r.GetInt32(o);
        return v >= 0 ? (uint)v : throw new OverflowException("Negative value cannot be converted to uint");
    }

    public static ulong ToUInt64(IDataRecord r, int o)
    {
        var v = r.GetInt64(o);
        return v >= 0 ? (ulong)v : throw new OverflowException("Negative value cannot be converted to ulong");
    }
}

/// <summary>
/// 每个实体一份的列绑定计划缓存基类，提供统一的 schema → plan 缓存。
/// 生成代码继承它，只实现 Bind 即可。
/// </summary>
public abstract class ReaderPlanCache<TPlan>
    where TPlan : class
{
    private readonly ConcurrentDictionary<string, TPlan> _cache = new();

    public TPlan Get(IDataReader reader)
    {
        var key = ReaderSchemaKey.Build(reader);
        return _cache.TryGetValue(key, out var plan)
            ? plan
            : _cache.GetOrAdd(key, _ => Bind(reader));
    }

    protected abstract TPlan Bind(IDataReader reader);
}

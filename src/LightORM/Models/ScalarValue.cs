using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LightORM.Models;

public static class ScalarValueExtension
{
    extension(ScalarValue scalarValue)
    {
        public T? As<T>()
        {
            if (scalarValue.IsNull)
            {
                return default;
            }
            return SqlExecutor.SqlExecutor.ChangeType<T>(scalarValue.Value);
        }
    }
}

public readonly record struct ScalarValue(object? Value)
{
    /// <summary>
    /// 是否为 DBNull 或 null
    /// </summary>
    public bool IsNull => Value is null or DBNull;

    /// <summary>
    /// 是否有值
    /// </summary>
    public bool HasValue => !IsNull;

    // string
    public static implicit operator string?(ScalarValue scalarValue) => scalarValue.As<string>();
    // int and int?
    public static implicit operator int(ScalarValue scalarValue) => scalarValue.As<int>();
    public static implicit operator int?(ScalarValue scalarValue) => scalarValue.As<int?>();
    // long and long?
    public static implicit operator long(ScalarValue scalarValue) => scalarValue.As<long>();
    public static implicit operator long?(ScalarValue scalarValue) => scalarValue.As<long?>();
    // short and short?
    public static implicit operator short(ScalarValue scalarValue) => scalarValue.As<short>();
    public static implicit operator short?(ScalarValue scalarValue) => scalarValue.As<short?>();
    // byte and byte?
    public static implicit operator byte(ScalarValue scalarValue) => scalarValue.As<byte>();
    public static implicit operator byte?(ScalarValue scalarValue) => scalarValue.As<byte?>();
    // decimal and decimal?
    public static implicit operator decimal(ScalarValue scalarValue) => scalarValue.As<decimal>();
    public static implicit operator decimal?(ScalarValue scalarValue) => scalarValue.As<decimal?>();
    // double and double?
    public static implicit operator double(ScalarValue scalarValue) => scalarValue.As<double>();
    public static implicit operator double?(ScalarValue scalarValue) => scalarValue.As<double?>();
    // float and float?
    public static implicit operator float(ScalarValue scalarValue) => scalarValue.As<float>();
    public static implicit operator float?(ScalarValue scalarValue) => scalarValue.As<float?>();
    // bool and bool?
    public static implicit operator bool(ScalarValue scalarValue) => scalarValue.As<bool>();
    public static implicit operator bool?(ScalarValue scalarValue) => scalarValue.As<bool?>();
    // DateTime and DateTime?
    public static implicit operator DateTime(ScalarValue scalarValue) => scalarValue.As<DateTime>();
    public static implicit operator DateTime?(ScalarValue scalarValue) => scalarValue.As<DateTime?>();
    // Guid and Guid?
    public static implicit operator Guid(ScalarValue scalarValue) => scalarValue.As<Guid>();
    public static implicit operator Guid?(ScalarValue scalarValue) => scalarValue.As<Guid?>();
    // char and char?
    public static implicit operator char(ScalarValue scalarValue) => scalarValue.As<char>();
    public static implicit operator char?(ScalarValue scalarValue) => scalarValue.As<char?>();
    // byte[]
    public static implicit operator byte[]?(ScalarValue scalarValue) => scalarValue.As<byte[]?>();
}

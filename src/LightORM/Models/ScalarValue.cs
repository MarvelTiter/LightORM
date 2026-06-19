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

}

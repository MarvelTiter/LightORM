using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LightORM.Providers.PostgreSQL.Utils
{
    internal static class FormatType
    {
        public static string TransformType(this Type type)
        {
            string? typeFullName;
            if (type.IsEnum)
            {
                typeFullName = Enum.GetUnderlyingType(type).FullName;
            }
            else
            {
                typeFullName = (Nullable.GetUnderlyingType(type) ?? type).FullName;
            }

            return typeFullName switch
            {
                "System.Boolean" => "BOOLEAN",
                "System.Byte" => "SMALLINT", // PostgreSQL 没有直接的 BYTE 类型
                "System.Int16" => "SMALLINT",
                "System.Int32" => "INTEGER",
                "System.Int64" => "BIGINT",
                "System.Single" => "REAL",
                "System.Double" => "DOUBLE PRECISION",
                "System.Decimal" => "NUMERIC",
                "System.DateTime" => "TIMESTAMP",
                "System.DateTimeOffset" => "TIMESTAMP WITH TIME ZONE",
                "System.Guid" => "UUID",
                "System.Byte[]" => "BYTEA",
                _ => "TEXT", // PostgreSQL 的 TEXT 类型适合任意长度字符串
            };
        }
    }
}

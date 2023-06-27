using System;
using System.Data;

namespace MDbContext.DbStruct
{
    internal class OracleDbTable : DbTableBase
    {
        internal override void BuildSql(IDbConnection connection, DbTable info)
        {
            throw new NotImplementedException();
        }

        internal override bool CheckTableExists(IDbConnection connection, DbTable dbTable)
        {
            throw new NotImplementedException();
        }

        internal override string ConvertToDbType(DbColumn type)
        {
            string? typeFullName = "";
            if (type.DataType.IsEnum)
            {
                typeFullName = Enum.GetUnderlyingType(type.DataType).FullName;
            }
            else
            {
                typeFullName = (Nullable.GetUnderlyingType(type.DataType) ?? type.DataType).FullName;
            }
            return typeFullName switch
            {
                "System.Boolean" => "Bit",
                "System.Byte" => "TinyInt",
                "System.Int16" => "SmallInt",
                "System.Int32" => "Int",
                "System.Int64" => "BigInt",
                "System.Single" => "Numeric",
                "System.Double" => "Float",
                "System.Decimal" => "Money",
                "System.DateTime" => "DateTime",
                "System.DateTimeOffset" => "DateTimeOffset",
                "System.Guid" => "UniqueIdentifier",
                "System.Byte[]" => "BLOB",
                "System.Object" => "Variant",
                _ => "NVarChar",
            };
        }
    }
}

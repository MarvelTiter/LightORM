using System;
using System.Data;

namespace MDbContext.DbStruct
{
    internal class OracleDbTable : DbTableBase
    {
        public OracleDbTable(TableGenerateOption option) : base(option)
        {

        }

        internal override string BuildColumn(DbColumn column)
        {
            throw new NotImplementedException();
        }

        internal override string BuildSql(DbTable info)
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

        internal override string DbEmphasis(string name)
        {
            throw new NotImplementedException();
        }
    }
}

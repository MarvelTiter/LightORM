using System;
using System.Data;

namespace MDbContext.DbStruct
{
    internal class MySqlDbTable : DbTableBase
    {
        public MySqlDbTable(TableGenerateOption option) : base(option)
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
            throw new NotImplementedException();
        }

        internal override string DbEmphasis(string name)
        {
            throw new NotImplementedException();
        }
    }
}

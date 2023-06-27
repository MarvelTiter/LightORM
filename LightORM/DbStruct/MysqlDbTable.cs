using System;
using System.Data;

namespace MDbContext.DbStruct
{
    internal class MySqlDbTable : DbTableBase
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
            throw new NotImplementedException();
        }
    }
}

using System;
using System.Data;

namespace MDbContext.DbStruct
{
    internal class SqlServerDbTable : IDbTable
    {
        public bool GenerateDbTable<T>(IDbConnection conn, out string message)
        {
            throw new NotImplementedException();
        }

        public void SaveDbTableStruct()
        {
            throw new NotImplementedException();
        }
    }
}

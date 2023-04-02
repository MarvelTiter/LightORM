using System;
using System.Data;

namespace MDbContext.DbStruct
{
    internal class OracleDbTable : IDbTable
    {
        public bool GenerateDbTable<T>(IDbConnection connection, out string message)
        {
            throw new NotImplementedException();
        }

        public void SaveDbTableStruct()
        {
            throw new NotImplementedException();
        }
    }
}

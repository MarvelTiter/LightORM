using MDbContext.Extension;
using System;
using System.Data;

namespace MDbContext.DbStruct
{
    internal abstract class DbTableBase: IDbTable
    {
        internal abstract bool CheckTableExists(IDbConnection connection, DbTable dbTable);
        internal abstract string ConvertToDbType(DbColumn type);
        internal abstract void BuildSql(IDbConnection connection, DbTable info);
        public bool GenerateDbTable<T>(IDbConnection connection, out string message)
        {
            try
            {
                var info = typeof(T).CollectDbTableInfo();
                if (!CheckTableExists(connection, info))
                {
                    BuildSql(connection, info);
                    message = string.Empty;
                }
                else
                {
                    message = "Table Exist";
                }
                return true;
            }
            catch (Exception ex)
            {
                message = ex.Message;
                return false;
            }
        }
        public virtual void SaveDbTableStruct()
        {
            throw new NotImplementedException();
        }

    }
}

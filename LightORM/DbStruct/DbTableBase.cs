using MDbContext.DbStruct;
using MDbContext.Extension;
using MDbContext.SqlExecutor;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;

namespace MDbContext.DbStruct
{
    internal abstract class DbTableBase : IDbTable
    {
        protected TableGenerateOption Option { get; }
        public DbTableBase(TableGenerateOption option)
        {
            Option = option;
        }
        internal abstract string ConvertToDbType(DbColumn type);
        internal abstract string BuildColumn(DbColumn column);
        internal abstract string DbEmphasis(string name);
        internal abstract string BuildSql(DbTable table);
        public string GenerateDbTable<T>()
        {
            try
            {
                var info = typeof(T).CollectDbTableInfo();
                return BuildSql(info);
            }
            catch (Exception)
            {
                throw;
            }
        }
        public virtual void SaveDbTableStruct()
        {
            throw new NotImplementedException();
        }

        protected static string GetIndexName(DbTable info, DbIndex index, int i)
        {
            return index.Name ?? $"{info.Name}_{string.Join("_", index.Columns)}_{i}";
        }

        protected static string GetPrimaryKeyName(IEnumerable<DbColumn> pks)
        {
            return $"PK_{string.Join("_", pks.Select(c => c.Name))}";
        }
    }
}

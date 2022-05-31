using MDbContext.NewExpSql.Providers;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace MDbContext.NewExpSql
{
    public class ExpressionSql : IExpSql
    {
        private readonly ConcurrentDictionary<string, ITableContext> tableContexts = new ConcurrentDictionary<string, ITableContext>();
        private readonly ConcurrentDictionary<string, DbConnectInfo> dbFactories = new ConcurrentDictionary<string, DbConnectInfo>();
        private string dbKey = "MainDb";
        internal ExpressionSql(ConcurrentDictionary<string, DbConnectInfo> dbFactories)
        {
            this.dbFactories = dbFactories;
        }

        (ITableContext context, DbConnectInfo info) GetDbInfos(string key)
        {
            if (dbFactories.TryGetValue(key, out var dbInfo))
            {
                if (!tableContexts.TryGetValue(key, out var dbContext))
                {
                    dbContext = new TableContext(dbInfo.DbBaseType);
                    tableContexts[key] = dbContext;
                }
                return (dbContext, dbInfo);
            }
            throw new ArgumentException($"{key}异常");
        }

        public IExpSelect<T> Select<T>() => new SelectProvider<T>(dbKey, GetDbInfos);

        public IExpInsert<T> Insert<T>()
        {
            throw new NotImplementedException();
        }

        public IExpUpdate<T> Update<T>()
        {
            throw new NotImplementedException();
        }

        public IExpDelete<T> Delete<T>()
        {
            throw new NotImplementedException();
        }

        public override string ToString()
        {
            return sqlCore?.ToString();
        }
    }
}

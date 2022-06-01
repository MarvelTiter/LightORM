using MDbContext.NewExpSql.Ado;
using MDbContext.NewExpSql.Interface;
using MDbContext.NewExpSql.Interface.Select;
using MDbContext.NewExpSql.Providers;
using MDbContext.NewExpSql.Providers.Select;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace MDbContext.NewExpSql
{
    internal class ExpressionSql : IExpSql
    {
        private readonly ConcurrentDictionary<string, ITableContext> tableContexts = new ConcurrentDictionary<string, ITableContext>();
        private readonly ConcurrentDictionary<string, DbConnectInfo> dbFactories;
        private IAdo ado;
        public IAdo Ado => ado;

        internal ExpressionSql(ConcurrentDictionary<string, DbConnectInfo> dbFactories)
        {
            this.dbFactories = dbFactories;
            ado = new AdoImpl(dbFactories);
        }
        
        internal ITableContext GetContext(string key)
        {
            if (dbFactories.TryGetValue(key, out var dbInfo))
            {
                if (!tableContexts.TryGetValue(key, out var dbContext))
                {
                    dbContext = new TableContext(dbInfo.DbBaseType);
                    tableContexts[key] = dbContext;
                }
                return dbContext;
            }
            throw new ArgumentException($"{key}异常");
        }

        internal DbConnectInfo GetDbInfo(string key)
        {
            if (dbFactories.TryGetValue(key, out var dbInfo))
            {
                return dbInfo;
            }
            throw new ArgumentException($"{key}异常");
        }

        public IExpSelect<T> Select<T>(string key = "MainDb") => new SelectProvider1<T>(key, GetContext, GetDbInfo(key));

        public IExpInsert<T> Insert<T>(string key = "MainDb") => new InsertProvider<T>(key, GetContext, GetDbInfo(key));

        public IExpUpdate<T> Update<T>(string key = "MainDb")
        {
            throw new NotImplementedException();
        }

        public IExpDelete<T> Delete<T>(string key = "MainDb")
        {
            throw new NotImplementedException();
        }


    }
}

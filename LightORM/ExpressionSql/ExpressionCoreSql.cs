using MDbContext.ExpressionSql.Ado;
using MDbContext.ExpressionSql.Interface;
using MDbContext.ExpressionSql.Interface.Select;
using MDbContext.ExpressionSql.Providers;
using MDbContext.ExpressionSql.Providers.Select;
using System;
using System.Collections.Concurrent;

namespace MDbContext.ExpressionSql
{
    internal class ExpressionCoreSql : IExpSql
    {
        private readonly ConcurrentDictionary<string, ITableContext> tableContexts = new ConcurrentDictionary<string, ITableContext>();
        private readonly ConcurrentDictionary<string, DbConnectInfo> dbFactories;
        private IAdo ado;
        public IAdo Ado => ado;

        internal ExpressionCoreSql(ConcurrentDictionary<string, DbConnectInfo> dbFactories)
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

        public IExpSelect<T> Select<T>(string key = ConstString.Main) => new SelectProvider1<T>(key, GetContext, GetDbInfo(key));

        public IExpInsert<T> Insert<T>(string key = ConstString.Main) => new InsertProvider<T>(key, GetContext, GetDbInfo(key));

        public IExpUpdate<T> Update<T>(string key = ConstString.Main) => new UpdateProvider<T>(key, GetContext, GetDbInfo(key));

        public IExpDelete<T> Delete<T>(string key = ConstString.Main) => new DeleteProvider<T>(key, GetContext, GetDbInfo(key));

    }
}

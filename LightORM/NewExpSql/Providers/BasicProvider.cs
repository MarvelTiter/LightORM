using MDbContext.NewExpSql.Interface;
using System;
using System.Collections.Generic;

namespace MDbContext.NewExpSql.Providers
{
    internal class BasicProvider<T>
    {
        protected readonly SqlContext context;
        protected readonly List<TableInfo> tables;
        protected readonly DbConnectInfo dbConnect;

        public BasicProvider(string key, Func<string, (ITableContext context, DbConnectInfo info)> getDbInfos)
        {
            var result = getDbInfos.Invoke(key);
            context = new SqlContext(result.context);
            tables = new List<TableInfo>();
            var main = context.AddTable(typeof(T));
            tables.Add(main);
            dbConnect = result.info;
        }
    }
}

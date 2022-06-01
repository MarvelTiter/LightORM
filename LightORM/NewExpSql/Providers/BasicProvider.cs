using MDbContext.NewExpSql.Interface;
using MDbContext.NewExpSql.Interface.Select;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace MDbContext.NewExpSql.Providers
{
    internal class BasicProvider<T1>
    {
        protected readonly SqlContext context;
        //protected readonly List<TableInfo> tables;
        protected readonly DbConnectInfo dbConnect;
        protected readonly Dictionary<string, SqlFieldInfo> SessionFields = new Dictionary<string, SqlFieldInfo>();

        public BasicProvider(string key, Func<string, ITableContext> getContext, DbConnectInfo connectInfos)
        {
            var tbContext = getContext.Invoke(key);
            dbConnect = connectInfos;
            context = new SqlContext(tbContext);
            //tables = new List<TableInfo>();
            var main = context.AddTable(typeof(T1));
        }
    }
}

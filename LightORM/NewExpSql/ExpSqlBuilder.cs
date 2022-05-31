using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;

namespace MDbContext.NewExpSql
{
    public partial class ExpSqlBuilder
    {
        ConcurrentDictionary<string, DbConnectInfo>? dbFactories = new ConcurrentDictionary<string, DbConnectInfo>();
        public ExpSqlBuilder SetDatabase(DbBaseType dbBaseType, Func<IDbConnection> dbFactory)
        {
            dbFactories["MainDb"] = new DbConnectInfo() { DbBaseType = dbBaseType, CreateConnection = dbFactory };
            return this;
        }
        public ExpSqlBuilder SetSalveDatabase(string key, DbBaseType dbBaseType, Func<IDbConnection> dbFactory)
        {
            if (key == "MainDb") throw new ArgumentException("key 不能为 MainDb");
            dbFactories[key] = new DbConnectInfo() { DbBaseType = dbBaseType, CreateConnection = dbFactory };
            return this;
        }
        public IExpSql Build()
        {
            if ((dbFactories?.Count ?? 0) < 1)
            {
                throw new Exception("未设置连接数据库");
            }
            return new ExpressionSql(dbFactories);
        }
    }
}

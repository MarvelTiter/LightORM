using System;
using System.Data;

namespace MDbContext.NewExpSql.Interface
{
    internal struct DbConnectInfo
    {
        public Func<IDbConnection> CreateConnection;
        public DbBaseType DbBaseType;
    }
}

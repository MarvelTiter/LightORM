using System;
using System.Data;

namespace MDbContext.NewExpSql.Interface
{
    internal struct DbConnectInfo
    {
        public Func<IDbConnection> CreateConnection { get; set; }
        public DbBaseType DbBaseType { get; set; }
    }
}

using System;
using System.Data;

namespace MDbContext.ExpressionSql.Interface
{
    internal struct DbConnectInfo
    {
        public Func<IDbConnection> CreateConnection { get; set; }
        public DbBaseType DbBaseType { get; set; }
    }
}

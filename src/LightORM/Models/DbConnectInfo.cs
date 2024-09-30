using System;
using System.Data;
using System.Data.Common;

namespace LightORM;

public record DbConnectInfo
{

    public DbConnectInfo(DbBaseType db, IDatabaseProvider provider) 
    {
        DbBaseType = db;
        Database = provider;
        //CreateConnection = func;
    }

    public DbBaseType DbBaseType { get; set; }
    public IDatabaseProvider Database { get; }
    public string ConnectString => Database.MasterConnectionString;
    public DbProviderFactory DbProviderFactory => Database.DbProviderFactory;

    //public Func<DbConnection>? CreateConnection { get; set; }
}

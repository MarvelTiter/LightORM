using System;
using System.Data;
using System.Data.Common;

namespace LightORM;

public record DbConnectInfo
{

    public DbConnectInfo(DbBaseType db, string connectString, DbProviderFactory factory) 
    {
        DbBaseType = db;
        ConnectString = connectString;
        DbProviderFactory = factory;
        //CreateConnection = func;
    }

    public DbBaseType DbBaseType { get; set; }
    public string ConnectString { get; set; }
    public DbProviderFactory DbProviderFactory { get; set; }

    //public Func<DbConnection>? CreateConnection { get; set; }
}

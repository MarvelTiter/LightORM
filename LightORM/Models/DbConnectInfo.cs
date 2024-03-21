using System;
using System.Data;
using System.Data.Common;

namespace LightORM.Models;

internal class DbConnectInfo
{
    public DbConnectInfo(DbBaseType db, string connectString, DbProviderFactory factory)
    {
        DbBaseType = db;
        ConnectString = connectString;
        DbProviderFactory = factory;
    }
    public Func<IDbConnection>? CreateConnection { get; set; }
    public DbBaseType DbBaseType { get; set; }
    public string ConnectString { get; set; }
    public DbProviderFactory DbProviderFactory { get; set; }
}

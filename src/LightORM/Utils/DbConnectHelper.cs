using LightORM.Cache;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LightORM.Utils
{
    public class DbConnectHelper
    {
        //public static void TryAddConnectionInfo(string key, DbBaseType dbBaseType, string connStr, DbProviderFactory factory)
        //{
        //    if (StaticCache<DbConnectInfo>.HasKey(key))
        //        return;
        //    var info = new DbConnectInfo(dbBaseType, connStr, factory);
        //    _ = StaticCache<DbConnectInfo>.GetOrAdd(key, () => info);
        //}
    }
}

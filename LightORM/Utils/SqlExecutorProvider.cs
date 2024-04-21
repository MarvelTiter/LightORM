﻿using LightORM.Cache;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LightORM.Utils
{
    public class SqlExecutorProvider
    {
        public static ISqlExecutor GetExecutor(string key = ConstString.Main)
        {
            var dbInfo = StaticCache<DbConnectInfo>.Get(key) ?? throw new LightOrmException($"{key} not register");
            return new SqlExecutor.SqlExecutor(dbInfo);
        }
    }
}
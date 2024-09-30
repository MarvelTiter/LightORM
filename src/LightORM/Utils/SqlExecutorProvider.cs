﻿using LightORM.Cache;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LightORM.Utils
{
    internal class SqlExecutorProvider : IDisposable
    {
        public static ISqlExecutor GetExecutor(string key = ConstString.Main)
        {
            var dbInfo = StaticCache<DbConnectInfo>.Get(key) ?? throw new LightOrmException($"{key} not register");
            return new SqlExecutor.SqlExecutor(dbInfo);
        }

        public static DbConnectInfo GetDbInfo(string key)
        {
            return StaticCache<DbConnectInfo>.Get(key) ?? throw new ArgumentException($"{key} not register");
        }

        private readonly ConcurrentDictionary<string, ISqlExecutor> executors = [];
        //private readonly List<ISqlExecutor> queryExecutors = [];
        private readonly SqlAopProvider sqlAop;
        Func<string, bool, ISqlExecutor>? customHandler;
        private static ConcurrentQueue<WeakReference<SqlExecutor.SqlExecutor>> selectWeaks = [];
        public SqlExecutorProvider(SqlAopProvider sqlAop)
        {
            this.sqlAop = sqlAop;
        }


        public void UseCustomExecutor(Func<string, bool, ISqlExecutor> customHandler)
        {
            this.customHandler = customHandler;
        }

        private ISqlExecutor? CreateCustomExecutor(string key, bool useTrans)
        {
            if (customHandler == null) return null;
            var e = customHandler.Invoke(key, useTrans);
            customHandler = null;
            return e;
        }

        public ConcurrentDictionary<string, ISqlExecutor> Executors => executors;

        public ISqlExecutor GetSqlExecutor(string key, bool useTrans) => CreateCustomExecutor(key, useTrans) ?? InternalCreator(key, useTrans);

        public ISqlExecutor GetSelectExecutor(string key)
        {
            var custom = CreateCustomExecutor(key, false);
            if (custom != null) return custom;
            while (selectWeaks.TryDequeue(out var weak))
            {
                if (weak.TryGetTarget(out var executor))
                {
                    if (executor.DbConnection.State != System.Data.ConnectionState.Closed)
                    {
                        return CreateExecutor();
                    }
                    else
                    {
                        return executor;
                    }
                }
            }

            return CreateExecutor();

            ISqlExecutor CreateExecutor()
            {
                var n = new SqlExecutor.SqlExecutor(GetDbInfo(key))
                {
                    DbLog = sqlAop.DbLog,
                };
                var w = new WeakReference<SqlExecutor.SqlExecutor>(n);
                selectWeaks.Enqueue(w);
                return n;
            }
        }

        private ISqlExecutor InternalCreator(string key, bool useTrans)
        {
            return executors.GetOrAdd(key, k =>
            {
                var ado = new SqlExecutor.SqlExecutor(GetDbInfo(k))
                {
                    DbLog = sqlAop.DbLog
                };
                if (useTrans)
                {
                    ado.BeginTran();
                }
                return ado;
            });
        }

        #region dispose
        private bool disposedValue;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    foreach (var item in executors.Values)
                    {
                        item.Dispose();
                    }
                    executors.Clear();
                    //foreach (var item in queryExecutors)
                    //{
                    //    item?.Dispose();
                    //}
                    //queryExecutors.Clear();
                }
                disposedValue = true;
            }
        }


        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
using LightORM.Cache;
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
        Func<string, bool, ISqlExecutor> sqlExecutorCreator;

        public SqlExecutorProvider(SqlAopProvider sqlAop)
        {
            this.sqlAop = sqlAop;
            sqlExecutorCreator = InternalCreator;
        }


        public void UseCustomExecutor(Func<string, bool, ISqlExecutor> customHandler)
        {
            sqlExecutorCreator = customHandler;
        }

        private void ResetCreator()
        {
            sqlExecutorCreator = InternalCreator;
        }

        public ConcurrentDictionary<string, ISqlExecutor> Executors => executors;

        public ISqlExecutor GetSqlExecutor(string key, bool useTrans)
        {
            var executor = sqlExecutorCreator.Invoke(key, useTrans);
            ResetCreator();
            return executor;
        }

        public ISqlExecutor GetSelectExecutor(string key, bool useCustom = false)
        {
            ISqlExecutor ado = GetSqlExecutor(key, false);
            if (ado.DbConnection.State == System.Data.ConnectionState.Open)
            {
                var info = GetDbInfo(key);
                var nado = new SqlExecutor.SqlExecutor(info);
                nado.DbLog = sqlAop.DbLog;

                return nado;
            }
            //queryExecutors.Add(ado);
            return ado;
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

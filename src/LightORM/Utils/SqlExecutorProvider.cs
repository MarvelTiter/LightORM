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
            var dbInfo = StaticCache<IDatabaseProvider>.Get(key) ?? throw new LightOrmException($"{key} not register");
            return new SqlExecutor.SqlExecutor(dbInfo, 5);
        }

        public static IDatabaseProvider GetDbInfo(string key)
        {
            return StaticCache<IDatabaseProvider>.Get(key) ?? throw new ArgumentException($"{key} not register");
        }

        Func<ISqlExecutor>? customHandler;
        private readonly ConcurrentDictionary<string, ISqlExecutor> executors = [];
        private readonly ExpressionSqlOptions option;
        public SqlExecutorProvider(ExpressionSqlOptions option)
        {
            this.option = option;
        }


        public void UseCustomExecutor(Func<ISqlExecutor> customHandler)
        {
            this.customHandler = customHandler;
        }

        private ISqlExecutor? CreateCustomExecutor()
        {
            if (customHandler == null) return null;
            var e = customHandler.Invoke();
            customHandler = null;
            return e;
        }

        public ConcurrentDictionary<string, ISqlExecutor> Executors => executors;

        public ISqlExecutor GetSqlExecutor(string key) => CreateCustomExecutor() ?? InternalCreator(key);

        private ISqlExecutor InternalCreator(string key)
        {
            return executors.GetOrAdd(key, k =>
            {
                var ado = new SqlExecutor.SqlExecutor(GetDbInfo(k),option.PoolSize,k)
                {
                    DbLog = option.Aop.DbLog
                };
                //if (useTrans)
                //{
                //    ado.BeginTran();
                //}
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

using LightORM.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LightORM.ExpressionSql
{
    public partial class ExpressionCoreSql : IExpressionContext
    {
        bool useTrans;
        public bool UseTrans { get => useTrans; set => useTrans = value; }

        public IExpressionContext Use(IDatabaseProvider db)
        {
            // 确保Use之后，拿到的ISqlExecutor是对应的
            switchSign.Wait();
            executorProvider.UseCustomExecutor((k, t) =>
            {
                var executor = new SqlExecutor.SqlExecutor(db, option.PoolSize);
                if (t)
                {
                    executor.BeginTran();
                }
                return executor;
            });
            return this;
        }
        public void BeginTranAll()
        {
            UseTrans = true;
            executorProvider.Executors.ForEach(ado =>
            {
                ado.BeginTran();
            });
        }

        public async Task BeginTranAllAsync()
        {
            UseTrans = true;
            await executorProvider.Executors.ForEachAsync(async ado =>
            {
                try { await ado.BeginTranAsync(); } catch { }
            });
        }

        public void CommitTranAll()
        {
            UseTrans = false;
            executorProvider.Executors.ForEach(ado =>
            {
                try { ado.CommitTran(); } catch { }
            });
        }

        public async Task CommitTranAllAsync()
        {
            UseTrans = false;
            await executorProvider.Executors.ForEachAsync(async ado =>
            {
                try { await ado.CommitTranAsync(); } catch { }
            });
        }

        public void RollbackTranAll()
        {
            UseTrans = false;
            executorProvider.Executors.ForEach(ado =>
            {
                try { ado.RollbackTran(); } catch { }
            });
        }

        public async Task RollbackTranAllAsync()
        {
            UseTrans = false;
            await executorProvider.Executors.ForEachAsync(async ado =>
            {
                try
                {
                    await ado.RollbackTranAsync();
                }
                catch { }
            });
        }


        public void BeginTran(string key = ConstString.Main)
        {
            try { executorProvider.GetSqlExecutor(key, false).BeginTran(); } catch { }
        }

        public async Task BeginTranAsync(string key = ConstString.Main)
        {
            try { await executorProvider.GetSqlExecutor(CurrentKey, false).BeginTranAsync(); } catch { }
        }

        public IScopedExpressionContext BeginScopedTran(string key = ConstString.Main)
        {
            var ado = executorProvider.GetSqlExecutor(key, false);
            ado.BeginTran();
            return new ScopedExpressionCoreSql(ado);
        }

        public async Task<IScopedExpressionContext> BeginScopedTranAsync(string key = ConstString.Main)
        {
            var ado = executorProvider.GetSqlExecutor(key, false);
            await ado.BeginTranAsync();
            return new ScopedExpressionCoreSql(ado);
        }

        public void CommitTran(string key = ConstString.Main)
        {
            try { executorProvider.GetSqlExecutor(key, false).CommitTran(); } catch { }
        }

        public async Task CommitTranAsync(string key = ConstString.Main)
        {
            try { await executorProvider.GetSqlExecutor(key, false).CommitTranAsync(); } catch { }
        }

        public void RollbackTran(string key = ConstString.Main)
        {
            try { executorProvider.GetSqlExecutor(key, false).RollbackTran(); } catch { }
        }

        public async Task RollbackTranAsync(string key = ConstString.Main)
        {
            try { await executorProvider.GetSqlExecutor(key, false).RollbackTranAsync(); } catch { }
        }


    }
}

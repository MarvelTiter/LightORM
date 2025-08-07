using LightORM.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LightORM.ExpressionSql
{
    partial class ExpressionCoreSql : IExpressionContext
    {

        public IExpressionContext Use(IDatabaseProvider db)
        {
            // 确保Use之后，拿到的ISqlExecutor是对应的
            switchSign.Wait();
            executorProvider.UseCustomExecutor(() => new SqlExecutor.SqlExecutor(db, Options.PoolSize,new AdoInterceptor(Options.Interceptors)));
            return this;
        }
        //public void BeginTranAll()
        //{
        //    UseTrans = true;
        //    executorProvider.Executors.ForEach(ado =>
        //    {
        //        ado.BeginTran();
        //    });
        //}

        //public async Task BeginTranAllAsync()
        //{
        //    UseTrans = true;
        //    await executorProvider.Executors.ForEachAsync(async ado =>
        //    {
        //        try { await ado.BeginTranAsync(); } catch { }
        //    });
        //}

        //public void CommitTranAll()
        //{
        //    UseTrans = false;
        //    executorProvider.Executors.ForEach(ado =>
        //    {
        //        try { ado.CommitTran(); } catch { }
        //    });
        //}

        //public async Task CommitTranAllAsync()
        //{
        //    UseTrans = false;
        //    await executorProvider.Executors.ForEachAsync(async ado =>
        //    {
        //        try { await ado.CommitTranAsync(); } catch { }
        //    });
        //}

        //public void RollbackTranAll()
        //{
        //    UseTrans = false;
        //    executorProvider.Executors.ForEach(ado =>
        //    {
        //        try { ado.RollbackTran(); } catch { }
        //    });
        //}

        //public async Task RollbackTranAllAsync()
        //{
        //    UseTrans = false;
        //    await executorProvider.Executors.ForEachAsync(async ado =>
        //    {
        //        try
        //        {
        //            await ado.RollbackTranAsync();
        //        }
        //        catch { }
        //    });
        //}


        //public void BeginTran(string key = ConstString.Main)
        //{
        //    try { executorProvider.GetSqlExecutor(key, false).BeginTran(); } catch { }
        //}

        //public async Task BeginTranAsync(string key = ConstString.Main)
        //{
        //    try { await executorProvider.GetSqlExecutor(key, false).BeginTranAsync(); } catch { }
        //}

        public ISingleScopedExpressionContext CreateScoped(string key)
        {
            Debug.WriteLine("CreateScoped");
            var ado = (ISqlExecutor)executorProvider.GetSqlExecutor(key).Clone();
            return new SingleScopedExpressionCoreSql(ado);
        }

        public IScopedExpressionContext CreateScoped()
        {
            return new ScopedExpressionCoreSql(Options);
        }

        //public void CommitTran(string key = ConstString.Main)
        //{
        //    try { executorProvider.GetSqlExecutor(key, false).CommitTran(); } catch { }
        //}

        //public async Task CommitTranAsync(string key = ConstString.Main)
        //{
        //    try { await executorProvider.GetSqlExecutor(key, false).CommitTranAsync(); } catch { }
        //}

        //public void RollbackTran(string key = ConstString.Main)
        //{
        //    try { executorProvider.GetSqlExecutor(key, false).RollbackTran(); } catch { }
        //}

        //public async Task RollbackTranAsync(string key = ConstString.Main)
        //{
        //    try { await executorProvider.GetSqlExecutor(key, false).RollbackTranAsync(); } catch { }
        //}


    }
}

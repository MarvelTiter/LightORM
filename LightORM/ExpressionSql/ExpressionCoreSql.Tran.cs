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

        public void BeginTran()
        {
            useTrans = true;
            executors.ForEach(ado =>
            {
                try { ado.BeginTran(); } catch { }
            });
        }

        public async Task BeginTranAsync()
        {
            useTrans = true;
            await executors.ForEachAsync(async ado =>
            {
                try
                {
                    await ado.BeginTranAsync();
                }
                catch { }
            });
        }

        public void CommitTran()
        {
            useTrans = false;
            executors.ForEach(ado =>
            {
                try { ado.CommitTran(); } catch { }
            });
        }

        public async Task CommitTranAsync()
        {
            useTrans = false;
            await executors.ForEachAsync(async ado =>
            {
                try
                {
                    await ado.CommitTranAsync();
                }
                catch { }
            });
        }

        public void RollbackTran()
        {
            useTrans = false;
            executors.ForEach(ado =>
            {
                try { ado.RollbackTran(); } catch { }
            });
        }

        public async Task RollbackTranAsync()
        {
            useTrans = false;
            await executors.ForEachAsync(async ado =>
            {
                try
                {
                    await ado.RollbackTranAsync();
                }
                catch { }
            });
        }


        public void BeginTran(string key)
        {
            try
            {
                GetExecutor(key).BeginTran();
            }
            catch
            {
            }
        }

        public async Task BeginTranAsync(string key)
        {
            try
            {
                await GetExecutor(key).BeginTranAsync();
            }
            catch
            {
            }
        }

        public void CommitTran(string key)
        {
            try
            {
                GetExecutor(key).CommitTran();
            }
            catch
            {
            }
        }

        public async Task CommitTranAsync(string key)
        {
            try
            {
                await GetExecutor(key).CommitTranAsync();
            }
            catch
            {
            }
        }

        public void RollbackTran(string key)
        {
            try
            {
                GetExecutor(key).RollbackTran();
            }
            catch
            {
            }
        }

        public async Task RollbackTranAsync(string key)
        {
            try
            {
                await GetExecutor(key).RollbackTranAsync();
            }
            catch
            {
            }
        }
    }
}

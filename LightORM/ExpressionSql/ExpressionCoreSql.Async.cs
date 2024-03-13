using LightORM.SqlExecutor;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace LightORM.ExpressionSql;

internal partial class ExpressionCoreSql
{
#if NET40
#else
    public async Task<bool> CommitTransactionAsync()
    {
        if (transactions.Count == 0) return false;
        var groups = transactions.GroupBy(i => i.Key);
        Dictionary<IDbConnection, IDbTransaction> allTrans = new Dictionary<IDbConnection, IDbTransaction>();
        foreach (var g in groups)
        {
            var conn = dbFactories[g.Key].CreateConnection();
            conn.Open();
            var trans = conn.BeginTransaction();
            allTrans.Add(conn, trans);
            foreach (var item in g)
            {
                try
                {
                    await conn.ExecuteAsync(item.Sql, item.Parameters, trans);
                }
                catch (Exception ex)
                {
                    foreach (var kv in allTrans)
                    {
                        kv.Value.Rollback();
                        kv.Key.Close();
                    }
                    throw ex;
                }
            }
        }
        foreach (var kv in allTrans)
        {
            kv.Value.Commit();
            kv.Key.Close();
        }
        return true;
    }
#endif
}

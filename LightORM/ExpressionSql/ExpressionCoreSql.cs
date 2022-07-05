﻿using MDbContext.ExpressionSql.Ado;
using MDbContext.ExpressionSql.Interface;
using MDbContext.ExpressionSql.Interface.Select;
using MDbContext.ExpressionSql.Providers;
using MDbContext.ExpressionSql.Providers.Select;
using MDbContext.SqlExecutor;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace MDbContext.ExpressionSql
{
    internal class ExpressionCoreSql : IExpressionContext, IDisposable
    {
        private readonly ConcurrentDictionary<string, ITableContext> tableContexts = new ConcurrentDictionary<string, ITableContext>();
        private readonly ConcurrentDictionary<string, DbConnectInfo> dbFactories;
        internal readonly SqlExecuteLife Life;
        private IAdo ado;

        public IAdo Ado => ado;

        internal ExpressionCoreSql(ConcurrentDictionary<string, DbConnectInfo> dbFactories, SqlExecuteLife life, IAdo? ado = null)
        {
            this.dbFactories = dbFactories;
            this.Life = life;
            this.Life.Core = this;
            this.ado = ado ?? new AdoImpl(dbFactories);
        }

        internal ITableContext GetContext(string key)
        {
            if (dbFactories.TryGetValue(key, out var dbInfo))
            {
                if (!tableContexts.TryGetValue(key, out var dbContext))
                {
                    dbContext = new TableContext(dbInfo.DbBaseType);
                    tableContexts[key] = dbContext;
                }
                return dbContext;
            }
            throw new ArgumentException($"{key}异常");
        }

        internal DbConnectInfo GetDbInfo(string key)
        {
            if (dbFactories.TryGetValue(key, out var dbInfo))
            {
                return dbInfo;
            }
            throw new ArgumentException($"{key}异常");
        }

        public IExpSelect<T> Select<T>(string key = ConstString.Main) => new SelectProvider1<T>(key, GetContext, GetDbInfo(key), Life);

        public IExpInsert<T> Insert<T>(string key = ConstString.Main) => new InsertProvider<T>(key, GetContext, GetDbInfo(key), Life);

        public IExpUpdate<T> Update<T>(string key = ConstString.Main) => new UpdateProvider<T>(key, GetContext, GetDbInfo(key), Life);

        public IExpDelete<T> Delete<T>(string key = ConstString.Main) => new DeleteProvider<T>(key, GetContext, GetDbInfo(key), Life);

        private struct TransactionInfo
        {
            public string Sql { get; set; }
            public object Parameters { get; set; }
            public string Key { get; set; }
        }
        List<TransactionInfo> transactions = new List<TransactionInfo>();
        internal void Attch(string sql, object param, string key = ConstString.Main)
        {
            transactions.Add(new TransactionInfo
            {
                Key = key,
                Sql = sql,
                Parameters = param,
            });
        }

        public IExpressionContext BeginTransaction()
        {
            return new ExpressionCoreSql(dbFactories, Life, ado);
        }

        public bool CommitTransaction()
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
                        conn.Execute(item.Sql, item.Parameters, trans);
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

        private bool disposedValue;
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // 释放托管状态(托管对象)
                }

                // 释放未托管的资源(未托管的对象)并重写终结器
                // 将大型字段设置为 null
                disposedValue = true;
            }
        }

        // // 仅当“Dispose(bool disposing)”拥有用于释放未托管资源的代码时才替代终结器
        // ~ExpressionCoreSql()
        // {
        //     // 不要更改此代码。请将清理代码放入“Dispose(bool disposing)”方法中
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            // 不要更改此代码。请将清理代码放入“Dispose(bool disposing)”方法中
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}

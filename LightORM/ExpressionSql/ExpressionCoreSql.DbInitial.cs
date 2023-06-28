using MDbContext.DbStruct;
using MDbContext.ExpressionSql.DbHandle;
using MDbContext.ExpressionSql.Interface;
using MDbContext.SqlExecutor;
using System;
using System.Collections.Generic;
using System.Data;

namespace MDbContext.ExpressionSql
{
    internal partial class ExpressionCoreSql : IDbInitial
    {

        public void Log(string message)
        {
#if NET6_0_OR_GREATER || NETCOREAPP3_1_OR_GREATER
            if (Logger != null)
                Microsoft.Extensions.Logging.LoggerExtensions.LogInformation(Logger, message);
#endif
        }
        public string GenerateCreateSql<T>(string key = ConstString.Main)
        {
            var info = GetDbInfo(key);
            var db = GetDbTable(key, info.DbBaseType);
            var sql = db.GenerateDbTable<T>();
            return sql;
        }
        public IDbInitial CreateTable<T>(string key = ConstString.Main)
        {
            var info = GetDbInfo(key);
            try
            {
                var sql = GenerateCreateSql<T>(key);
                using var conn = info.CreateConnection.Invoke();
                conn.Execute(sql);
                Log($"{info.DbBaseType} Table: [{typeof(T).Name}] Created!");
            }
            catch (Exception ex)
            {
                Log($"{info.DbBaseType} Table: [{typeof(T).Name}] Create Failed! {ex.Message}");
                throw;
            }
            return this;
        }


        TableGenerateOption tableOption = new TableGenerateOption();
        public IDbInitial Configuration(Action<TableGenerateOption> option)
        {
            option?.Invoke(tableOption);
            return this;
        }
        private static Dictionary<string, IDbTable> caches = new();
        private IDbTable GetDbTable(string key, DbBaseType dbType)
        {
            if (!caches.TryGetValue(key, out var db))
            {
                db = dbType.GetDbTable(tableOption);
                caches[key] = db;
            }
            return db;
        }


    }
}

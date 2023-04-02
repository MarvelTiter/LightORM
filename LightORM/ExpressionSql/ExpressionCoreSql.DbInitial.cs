using MDbContext.DbStruct;
using MDbContext.ExpressionSql.DbHandle;
using MDbContext.ExpressionSql.Interface;
using System;

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

        public IDbInitial CreateTable<T>(string key = ConstString.Main)
        {
            var info = GetDbInfo(key);
            using var conn = info.CreateConnection();
            var db = info.DbBaseType.GetDbTable();
            if (db.GenerateDbTable<T>(conn, out var msg))
            {
                Log($"{info.DbBaseType} Table: [{typeof(T).Name}] Created! {msg}");
            }
            else
            {
                Log($"{info.DbBaseType} Table: [{typeof(T).Name}] Create Failed! {msg}");
            }
            return this;
        }
    }
}

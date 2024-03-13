using LightORM.Context;
using LightORM.DbStruct;
using LightORM.ExpressionSql.Interface;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace LightORM.ExpressionSql;

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
    public IDbInitial CreateTable<T>(string key = ConstString.Main, params T[]? datas)
    {
        var info = GetDbInfo(key);
        try
        {
            using var conn = info.CreateConnection.Invoke();
            DoCreateTable(conn, GenerateCreateSql<T>(key));
            Log($"{info.DbBaseType} Table: [{typeof(T).Name}] Created!");
            if (datas != null && datas.Any())
            {
                var effects = Insert<T>().AppendData(datas).Execute();
                Log($"{info.DbBaseType} Table: [{typeof(T).Name}] Inserted {effects} Rows");
            }
        }
        catch (Exception ex)
        {
            Log($"{info.DbBaseType} Table: [{typeof(T).Name}] Create Failed! {ex.Message}");
            throw;
        }
        return this;
    }

    private void DoCreateTable(IDbConnection conn, string sql)
    {
        var sqlArgs = new SqlArgs { Action = SqlAction.CreateTable, Sql = sql };
        Life.BeforeExecute?.Invoke(sqlArgs);
        conn.Execute(sql);
        sqlArgs.Done = true;
        Life.AfterExecute?.Invoke(sqlArgs);
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

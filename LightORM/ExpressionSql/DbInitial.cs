﻿//using LightORM.DbStruct;
//using LightORM.Interfaces;
//using System.Collections.Generic;
//using System.Data;
//using System.Linq;

//namespace LightORM.ExpressionSql;

//public class DbInitial : IDbInitial
//{
//    public void Log(string message)
//    {
//        //if (Logger != null)
//        //    Microsoft.Extensions.Logging.LoggerExtensions.LogInformation(Logger, message);
//    }
//    public string GenerateCreateSql<T>(ISqlExecutor executor)
//    {
//        var info = GetDbInfo(key);
//        var db = GetDbTable(key, info.DbBaseType);
//        var sql = db.GenerateDbTable<T>();
//        return sql;
//    }
//    public IDbInitial CreateTable<T>(string key = ConstString.Main, params T[]? datas)
//    {
//        using var info = SqlExecutorProvider.GetExecutor(key);
//        try
//        {
//            DoCreateTable(info, GenerateCreateSql<T>(key));
//            Log($"{info.ConnectInfo.DbBaseType} Table: [{typeof(T).Name}] Created!");
//            if (datas != null && datas.Any())
//            {
//                var effects = Insert<T>(datas).Execute();
//                Log($"{info.ConnectInfo.DbBaseType} Table: [{typeof(T).Name}] Inserted {effects} Rows");
//            }
//        }
//        catch (Exception ex)
//        {
//            Log($"{info.ConnectInfo.DbBaseType} Table: [{typeof(T).Name}] Create Failed! {ex.Message}");
//            throw;
//        }
//        return this;
//    }

//    private void DoCreateTable(ISqlExecutor conn, string sql)
//    {
//        conn.ExecuteNonQuery(sql);
//    }

//    TableGenerateOption tableOption = new TableGenerateOption();
//    public IDbInitial Configuration(Action<TableGenerateOption> option)
//    {
//        option?.Invoke(tableOption);
//        return this;
//    }
//    private static Dictionary<string, IDbTable> caches = new();
//    private IDbTable GetDbTable(string key, DbBaseType dbType)
//    {
//        if (!caches.TryGetValue(key, out var db))
//        {
//            db = dbType.GetDbTable(tableOption);
//            caches[key] = db;
//        }
//        return db;
//    }


//}

using LightORM.Cache;
using LightORM.DbStruct;
using LightORM.Interfaces;
using LightORM.Providers;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace LightORM.ExpressionSql;

public class DbInitial : IDbInitial
{
    private readonly ISqlExecutor executor;
    private IDbTable? dbTable;
    private IDbTable DbTable => dbTable ??= executor.ConnectInfo.DbBaseType.GetDbTable(tableOption);
    public DbInitial(ISqlExecutor executor)
    {
        this.executor = executor;
    }

    TableGenerateOption tableOption = new TableGenerateOption();
    public IDbInitial Configuration(Action<TableGenerateOption> option)
    {
        option?.Invoke(tableOption);
        dbTable = executor.ConnectInfo.DbBaseType.GetDbTable(tableOption);
        return this;
    }

    public IDbInitial CreateTable<T>(params T[]? datas)
    {
        try
        {
            var sql = DbTable.GenerateDbTable<T>();
            executor.ExecuteNonQuery(sql);
            if (datas?.Length > 0)
            {
                var insert = new InsertProvider<T>(executor, datas);
                var effects = insert.Execute();
            }
        }
        catch
        {
            throw;
        }
        return this;
    }
}

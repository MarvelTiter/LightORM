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
    private readonly IDatabaseTableHandler handler;
    public DbInitial(ISqlExecutor executor, IDatabaseTableHandler handler)
    {
        this.executor = executor;
        this.handler = handler;
    }

    TableGenerateOption tableOption = new TableGenerateOption();
    public IDbInitial Configuration(Action<TableGenerateOption> option)
    {
        option?.Invoke(tableOption);
        return this;
    }

    public IDbInitial CreateTable<T>(params T[]? datas)
    {
        try
        {
            var sql = handler.GenerateDbTable<T>();
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

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
    private readonly Func<TableGenerateOption, IDatabaseTableHandler> handler;
    private IDatabaseTableHandler? tableHandler;
    public DbInitial(ISqlExecutor executor, Func<TableGenerateOption, IDatabaseTableHandler> handler)
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
            tableHandler ??= handler.Invoke(tableOption);
            var sql = tableHandler.GenerateDbTable<T>();
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

    public IDbInitial CreateOrUpdateTable<T>(params T[]? datas)
    {
        throw new NotImplementedException();
    }
}

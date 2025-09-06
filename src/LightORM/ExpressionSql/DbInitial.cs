using LightORM.Providers;

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

    readonly TableGenerateOption tableOption = new TableGenerateOption();

    public IDbInitial Configuration(Action<TableGenerateOption> option)
    {
        option?.Invoke(tableOption);
        return this;
    }

    public IDbInitial CreateTable<T>(params T[]? datas)
    {
        try
        {
            var sql = handler.GenerateDbTable<T>(tableOption);
            foreach (var s in sql)
            {
                executor.ExecuteNonQuery(s);
            }

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
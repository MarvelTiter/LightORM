using LightORM.Providers;

namespace LightORM.ExpressionSql;

public class DbInitial(ISqlExecutor executor, IDatabaseTableHandler handler) : IDbInitial
{
    private readonly TableOptions tableOption = new();
    public IDbInitial Configuration(Action<TableOptions> option)
    {
        option?.Invoke(tableOption);
        return this;
    }

    public IDbInitial CreateTable<T>(params T[]? datas)
    {
        try
        {
            var sql = handler.GenerateDbTable<T>();
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
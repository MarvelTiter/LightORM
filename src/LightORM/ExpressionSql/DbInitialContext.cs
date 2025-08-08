using LightORM.Providers;

namespace LightORM;

public abstract class DbInitialContext
{
    //internal static MethodInfo InitializedMethod = typeof(ExpressionContext).GetMethod(nameof(ExpressionContext.Initialized))!;
    public abstract void Initialized(IDbInitial db);
    public virtual string DatabaseKey() => ConstString.Main;
    public DbInfo? Info { get; set; }
    internal void Check(ExpressionSqlOptions option)
    {
        bool hasTable = true;
        bool update = false;
        var key = DatabaseKey();
        if (!option.DatabaseProviders.TryGetValue(key, out var db))
        {
            throw new LightOrmException($"{key} not register");
        }
        using var executor = new SqlExecutor.SqlExecutor(db, new AdoInterceptor(option.Interceptors));
        option.DatabaseHandlers.TryGetValue(key, out var handler);
        if (handler?.Factory == null)
        {
            return;
        }

        try
        {
            var d = executor.Query<DbInfo>("SELECT * FROM DB_INITIAL_INFO").FirstOrDefault();
            if (d != null)
            {
                update = true;
                Info = d;
            }
        }
        catch
        {
            hasTable = false;
        }
        var context = new DbInitial(executor, handler.Factory);
        Info ??= new DbInfo();
        if (!hasTable)
        {
            context.CreateTable<DbInfo>();
        }
        if (!Info.Initialized)
        {
            Initialized(context);
            Info!.Initialized = true;
            if (update)
            {
                executor.ExecuteNonQuery("UPDATE DB_INITIAL_INFO SET INITIALIZED = '1'");
            }
            else
            {
                var insert = new InsertProvider<DbInfo>(executor, Info);
                insert.Execute();
            }
        }
    }
}
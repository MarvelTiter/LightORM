using LightORM.Cache;
using LightORM.ExpressionSql;
using LightORM.Providers;
using LightORM.Providers.Select;
using System.Linq;

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
        using var executor = SqlExecutorProvider.GetExecutor(DatabaseKey());
        executor.DbLog = option.Aop.DbLog;
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
        var context = new DbInitial(executor);
        Info ??= new DbInfo();
        if (!hasTable)
        {
            context.CreateTable<DbInfo>();
            Info.Initialized = false;
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
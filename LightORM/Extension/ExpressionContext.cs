using LightORM.DbEntity;
using LightORM.ExpressionSql.Interface;
using LightORM.ExpressionSql;
using System.Linq;
using System.Reflection;

namespace LightORM.Extension;

public abstract class DbInitialContext
{
    //internal static MethodInfo InitializedMethod = typeof(ExpressionContext).GetMethod(nameof(ExpressionContext.Initialized))!;
    public abstract void Initialized(IDbInitial db);
    public DbInfo? Info { get; set; }
    internal void Check(ExpressionCoreSql context)
    {
        try
        {
            var d = context.Select<DbInfo>().ToList().FirstOrDefault();
            if (d != null)
                Info = d;
        }
        catch
        {
            context.CreateTable<DbInfo>();
        }
        if (!Info!.Initialized)
        {
            Initialized(context);
            Info!.Initialized = true;
            context.Insert<DbInfo>().AppendData(Info).Execute();
        }
    }
}
using LightORM.ExpressionSql;
using System.Linq;

namespace LightORM;

public abstract class DbInitialContext
{
    //internal static MethodInfo InitializedMethod = typeof(ExpressionContext).GetMethod(nameof(ExpressionContext.Initialized))!;
    public abstract void Initialized(IDbInitial db);
    public DbInfo? Info { get; set; }
    internal void Check(ExpressionCoreSql context)
    {
        //TODO 数据库初始化
        //try
        //{
        //    var d = context.Select<DbInfo>().ToList().FirstOrDefault();
        //    if (d != null)
        //        Info = d;
        //}
        //catch
        //{
        //    context.CreateTable<DbInfo>();
        //}
        //if (!Info!.Initialized)
        //{
        //    Initialized(context);
        //    Info!.Initialized = true;
        //    context.Insert(Info).Execute();
        //}
    }
}
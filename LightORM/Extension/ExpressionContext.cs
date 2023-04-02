using MDbContext.ExpressionSql.Interface;
using System.Reflection;

namespace MDbContext
{
    public abstract class ExpressionContext
    {
        internal static MethodInfo InitializedMethod = typeof(ExpressionContext).GetMethod(nameof(ExpressionContext.Initialized))!;
        public abstract void Initialized(IDbInitial db);
    }
}
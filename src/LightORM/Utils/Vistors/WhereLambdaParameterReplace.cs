using LightORM.Performances;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LightORM.Utils.Vistors;

internal class WhereLambdaParameterReplace : ExpressionVisitor, IResetable
{
    private readonly List<ParameterExpression> newParameters = [];
    public static WhereLambdaParameterReplace Default => ExpressionVisitorPool<WhereLambdaParameterReplace>.Rent();
#if NET8_0_OR_GREATER
    [UnconditionalSuppressMessage("AOT", "IL3050", Justification = "重构Where表达式的参数部分")]
#endif
    public LambdaExpression Replace(LambdaExpression lambda, SelectBuilder select)
    {
        try
        {
            var p = lambda.Parameters;
            newParameters.AddRange(select.AllTables().Select(t => Expression.Parameter(t.Type, t.Alias)));
            var newBody = Visit(lambda.Body);
            return Expression.Lambda(newBody, newParameters);
        }
        finally
        {
            ExpressionVisitorPool<WhereLambdaParameterReplace>.Return(this);
        }
    }

    protected override Expression VisitParameter(ParameterExpression node)
    {
        foreach (var item in newParameters)
        {
            if (item.Type == node.Type)
                return item;
        }
        throw new LightOrmException($"调用了Where<{node.Type.Name}>(Expression<Func<{node.Type.Name}, bool>>)，但是{node.Type.Name}既没有被Select也没有被Join");
    }

    public void Reset()
    {
        newParameters.Clear();
    }
}

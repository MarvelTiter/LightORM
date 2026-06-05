using LightORM;
using LightORM.Performances;
using System.Diagnostics.CodeAnalysis;

namespace LightORM.Utils.Vistors;

internal class FlatTypeSet : ExpressionVisitor, IResetable
{
    private ParameterExpression[] parameters = [];
    private ParameterExpression? lambdaTypeSet;
    public static FlatTypeSet Default => ExpressionVisitorPool<FlatTypeSet>.Rent();
    public void Reset()
    {
        parameters = [];
        lambdaTypeSet = null;
    }
    public Expression? Flat(LambdaExpression exp)
    {
        try
        {
            var p = exp.Parameters;
            // TypeSet<T1,T2,....> 
            if (p.Count == 1 && p[0].Type.Name.StartsWith("TypeSet`"))
            {
                var set = p[0];
                parameters = [.. set.Type.GetGenericArguments().Select((p, i) => Expression.Parameter(p, $"p{i}"))];
                lambdaTypeSet = p[0];
            }
            else
            {
                return exp;
            }
            var body = Visit(exp.Body);
            return SafeCreateLambda(body, parameters);
        }
        finally
        {
            ExpressionVisitorPool<FlatTypeSet>.Return(this);
        }
    }

    /// <summary>
    /// 将属性访问的节点替换掉
    /// </summary>
    /// <param name="node"></param>
    /// <returns></returns>

    protected override Expression VisitMember(MemberExpression node)
    {
        int widx;

        if (node.Expression?.NodeType == ExpressionType.MemberAccess)
        {
            // w.TbX.XXX 或者 w.Tables.TbX.XXX
            var parent = node.Expression as MemberExpression;
            if (parent?.Expression?.Type.Name.StartsWith("TypeSet`") == true &&
                int.TryParse(parent.Member.Name.Replace("Tb", ""), out widx) && widx > 0)
            {
                if (parameters[widx - 1].Type != parent.Type) //解决 BaseEntity + AsTable 时报错
                    parameters[widx - 1] = Expression.Parameter(parent.Type, parameters[widx - 1].Name);
                return SafeCreateProperty(parameters[widx - 1], node.Member.Name);
            }
        }
        // w
        if (node.Expression?.NodeType == ExpressionType.Parameter &&
            node.Expression.Type.Name.StartsWith("TypeSet`") == true &&
            node.Expression == lambdaTypeSet &&
            int.TryParse(node.Member.Name.Replace("Tb", ""), out widx) && widx > 0)
        {
            if (parameters[widx - 1].Type != node.Type)
                parameters[widx - 1] = Expression.Parameter(node.Type, parameters[widx - 1].Name);
            return parameters[widx - 1];
        }
        return base.VisitMember(node);
    }


#if NET8_0_OR_GREATER
    [UnconditionalSuppressMessage("Trimming", "IL3050", Justification = "TypeSet 的泛型参数和成员由用户显式编写，AOT 安全")]
#endif
    private static LambdaExpression SafeCreateLambda(Expression body, ParameterExpression[] parameters)
    {
        return Expression.Lambda(body, parameters);
    }

#if NET8_0_OR_GREATER
    [UnconditionalSuppressMessage("Trimming", "IL2026", Justification = "TypeSet 的成员由用户显式编写，AOT 静态分析可跟踪")]
#endif
    private static MemberExpression SafeCreateProperty(Expression expression, string propertyName)
    {
        return Expression.Property(expression, propertyName);
    }
}

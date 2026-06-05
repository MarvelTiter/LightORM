using LightORM;
using LightORM.Performances;
using System.Diagnostics.CodeAnalysis;

namespace LightORM.Utils.Vistors;

internal class FlatGrouping : ExpressionVisitor, IResetable
{
    private List<ParameterExpression> parameters = [];
    private ParameterExpression? lambdaTypeSet;
    private LambdaExpression? keySelector;
    private int groupTypeIndex = 0;
    public static FlatGrouping Default => ExpressionVisitorPool<FlatGrouping>.Rent();

    public void Reset()
    {
        parameters.Clear();
        lambdaTypeSet = null;
        keySelector = null;
        groupTypeIndex = 0;
    }

    public LambdaExpression? Flat(LambdaExpression exp, LambdaExpression keySelector)
    {
        try
        {
            var p = exp.Parameters;
            // IExpSelectGrouping<TGroup, TTables>
            if (p.Count == 1 && p[0].Type.Name.StartsWith("IExpSelectGrouping"))
            {
                var gType = p[0].Type.GenericTypeArguments[0];
                // 并且 TTables 是 TypeSet<T1,T2,....> 
                if (p.Count == 1 && p[0].Type.GenericTypeArguments[1].Name.StartsWith("TypeSet`"))
                {
                    var tp = p[0].Type.GenericTypeArguments[1].GetGenericArguments().Select((p, i) => Expression.Parameter(p, $"p{i}")).ToArray();
                    groupTypeIndex = tp.Length;
                    parameters = [.. tp, p[0]];
                }
                else
                {
                    groupTypeIndex = 1;
                    var type = p[0].Type.GenericTypeArguments[1];
                    parameters = [Expression.Parameter(type, "p0"), p[0]];
                }
            }
            else if (p.Count == 1 && p[0].Type.Name.StartsWith("IGrouping"))
            {
                parameters = [.. p];
            }
            else
            {
                return exp;
            }
            this.keySelector = keySelector;
            TryAddSelectorParameters();
            var body = Visit(exp.Body);
            return SafeCreateLambda(body, parameters);
        }
        finally
        {
            ExpressionVisitorPool<FlatGrouping>.Return(this);
        }
    }

    private void TryAddSelectorParameters()
    {
        if (keySelector is null)
        {
            return;
        }
        foreach (var item in keySelector.Parameters)
        {
            if (!parameters.Any(p => p.Type == item.Type))
            {
                parameters.Add(item);
            }
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
            // w.Group.XXX 或者 w.Key.XXX
            else if (parent?.Member.Name == "Group" || parent?.Member.Name == "Key")
            {
                //if (parameters[groupTypeIndex].Type != parent.Type)
                //{
                //    parameters[groupTypeIndex] = Expression.Parameter(parent.Type, parameters[groupTypeIndex].Name);
                //}
                //return Expression.Property(parameters[groupTypeIndex], node.Member.Name);
                if (keySelector?.Body is NewExpression n)
                {
                    for (int i = 0; i < n.Arguments.Count; i++)
                    {
                        if (n.Members?[i].Name == node.Member.Name)
                        {
                            return n.Arguments[i];
                        }
                    }
                }
            }
            // w.Tables.XXX
            else if (parent?.Member.Name == "Tables")
            {
                if (parameters[0].Type != parent.Type)
                {
                    parameters[0] = Expression.Parameter(parent.Type, parameters[0].Name);
                }
                return SafeCreateProperty(parameters[0], node.Member.Name);
            }
        }


        if (node.Expression?.NodeType == ExpressionType.Parameter)
        {
            // w
            if (node.Expression.Type.Name.StartsWith("TypeSet`") == true &&
                node.Expression == lambdaTypeSet &&
                int.TryParse(node.Member.Name.Replace("Tb", ""), out widx) && widx > 0)
            {
                if (parameters[widx - 1].Type != node.Type)
                    parameters[widx - 1] = Expression.Parameter(node.Type, parameters[widx - 1].Name);
                return parameters[widx - 1];
            }
            // g.Group 或者 g.Key
            if (node.Member.Name == "Group" || node.Member.Name == "Key")
            {
                if (keySelector?.Body is MemberExpression m)
                {
                    return m;
                }
                else
                {
                    throw new LightOrmException("语法错误，Group是匿名类型，不能Select整个Group结果");
                }
            }

        }

        return base.VisitMember(node);
    }

#if NET8_0_OR_GREATER
    [UnconditionalSuppressMessage("Trimming", "IL3050", Justification = "TypeSet或IExpSelectGrouping 的泛型参数和成员由用户显式编写，AOT 安全")]
#endif
    private static LambdaExpression SafeCreateLambda(Expression body, IEnumerable<ParameterExpression> parameters)
    {
        return Expression.Lambda(body, parameters);
    }

#if NET8_0_OR_GREATER
    [UnconditionalSuppressMessage("Trimming", "IL2026", Justification = "TypeSet或IExpSelectGrouping 的成员由用户显式编写，AOT 静态分析可跟踪")]
    [UnconditionalSuppressMessage("Trimming", "IL2070", Justification = "TypeSet或IExpSelectGrouping 的成员由用户显式编写，AOT 安全")]
    [UnconditionalSuppressMessage("Trimming", "IL2075", Justification = "TypeSet或IExpSelectGrouping 的成员由用户显式编写，AOT 安全")]
#endif
    private static MemberExpression SafeCreateProperty(Expression expression, string propertyName)
    {
        return Expression.Property(expression, propertyName);
    }
}

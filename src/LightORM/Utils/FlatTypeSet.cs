namespace LightORM.Utils;

internal class FlatTypeSet : ExpressionVisitor
{
    private ParameterExpression[] parameters = [];
    private ParameterExpression? lambdaTypeSet;
    public static FlatTypeSet Default => new();

    public Expression? Flat(LambdaExpression exp)
    {
        var p = exp.Parameters;
        // TypeSet<T1,T2,....> 
        if (p.Count == 1 && p[0].Type.Name.StartsWith("TypeSet`"))
        {
            var set = p[0];
            parameters = [.. set.Type.GetProperties().Where(p => p.Name.StartsWith("Tb")).Select((p, i) => Expression.Parameter(p.PropertyType, $"p{i}"))];
            lambdaTypeSet = p[0];
        }
        //// IExpSelectGrouping<TGroup, TTables>
        //else if (p.Count == 1 && p[0].Type.Name.StartsWith("IExpSelectGrouping"))
        //{
        //    var gType = p[0].Type.GenericTypeArguments[0];
        //    // 并且 TTables 是 TypeSet<T1,T2,....> 
        //    if (p.Count == 1 && p[0].Type.GenericTypeArguments[1].Name.StartsWith("TypeSet`"))
        //    {
        //        var type = p[0].Type.GenericTypeArguments[1];
        //        var tp = type.GetProperties().Where(p => p.Name.StartsWith("Tb")).Select((p, i) => Expression.Parameter(p.PropertyType, $"p{i}")).ToArray();
        //        groupTypeIndex = tp.Length;
        //        parameters = [.. tp, Expression.Parameter(gType, "group")];
        //    }
        //    else
        //    {
        //        groupTypeIndex = 1;
        //        var type = p[0].Type.GenericTypeArguments[1];
        //        parameters = [Expression.Parameter(type, "p0"), Expression.Parameter(gType, "group")];
        //    }
        //}
        else
        {
            return exp;
        }
        var body = Visit(exp.Body);
        return Expression.Lambda(body, parameters);
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
                return Expression.Property(parameters[widx - 1], node.Member.Name);
            }
            //// w.Group.XXX
            //else if (parent?.Member.Name == "Group")
            //{
            //    if (parameters[groupTypeIndex].Type != parent.Type)
            //    {
            //        parameters[groupTypeIndex] = Expression.Parameter(parent.Type, parameters[groupTypeIndex].Name);
            //    }
            //    return Expression.Property(parameters[groupTypeIndex], node.Member.Name);
            //}
            //// w.Tables.XXX
            //else if (parent?.Member.Name == "Tables")
            //{
            //    if (parameters[0].Type != parent.Type)
            //    {
            //        parameters[0] = Expression.Parameter(parent.Type, parameters[0].Name);
            //    }
            //    return Expression.Property(parameters[0], node.Member.Name);
            //}
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
}

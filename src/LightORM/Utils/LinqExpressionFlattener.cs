using System.Reflection;

namespace LightORM.Utils;

internal class LinqExpressionFlattener : ExpressionVisitor
{
    private List<ParameterExpression> newParameters = [];
    private ParameterExpression? _transparentParameter;
    public static LinqExpressionFlattener Default => new();
    public LambdaExpression Flatten(LambdaExpression lambda)
    {
        var p = lambda.Parameters;
        // 检查是否是透明标识符模式
        if (p.Count == 1 &&
            p[0].Name?.Contains("TransparentIdentifier") == true)
        {
            _transparentParameter = lambda.Parameters[0];

            //// 从透明标识符中提取原始参数
            //if (lambda.Body is MemberExpression memberExpr && memberExpr.Expression is not null)
            //{
            //    // 创建原始参数表达式
            //    _originalParameter = Expression.Parameter(memberExpr.Expression.Type, "p");
            //    // 访问并转换表达式体
            //    // 返回新的Lambda表达式
            //}
            var newBody = Visit(lambda.Body);
            return Expression.Lambda(newBody, newParameters);
        }
        // IGrouping<TKey, TTables>
        else if (p.Count == 1 && p[0].Type.Name.StartsWith("IGrouping"))
        {

        }
        // 不是透明标识符模式，直接返回原表达式
        return lambda;
    }

    protected override Expression VisitMember(MemberExpression node)
    {
        // 处理透明标识符成员访问
        if (node.Expression?.NodeType == ExpressionType.MemberAccess)
        {

            // <>h__TransparentIdentifier0.u.Property → u.Property
            var parentMember = node.Expression as MemberExpression;
            if (parentMember?.Expression?.NodeType == ExpressionType.Parameter)
            {
                if (!newParameters.Any(p => p.Name == parentMember?.Member.Name))
                {
                    newParameters.Add(Expression.Parameter(GetMemberValueType(parentMember.Member), parentMember.Member.Name));
                }
                //return Expression.Property(
                //    _originalParameter!,
                //    node.Member.Name);
            }
            var pe = newParameters.FirstOrDefault(p => p.Name == parentMember?.Member.Name);
            if (pe is not null)
            {
                return Expression.Property(pe, node.Member.Name);
            }
        }

        if (node.Expression?.NodeType == ExpressionType.Parameter)
        {
            //return _originalParameter!;
        }

        return base.VisitMember(node);
    }

    private static Type GetMemberValueType(MemberInfo member)
    {
        if (member is PropertyInfo p)
        {
            return p.PropertyType;
        }
        else if (member is FieldInfo field)
        {
            return field.FieldType;
        }
        throw new InvalidOperationException();
    }
}
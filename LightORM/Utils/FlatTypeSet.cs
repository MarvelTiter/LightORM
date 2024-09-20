using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LightORM.Utils
{
    internal class FlatTypeSet : ExpressionVisitor
    {
        private ParameterExpression[] parameters = [];
        private ParameterExpression? lambdaTypeSet;

        public static FlatTypeSet Default => new();

        public Expression? Flat(LambdaExpression exp)
        {
            var p = exp.Parameters;
            if (p.Count == 1 && p[0].Type.Name.StartsWith("TypeSet`"))
            {
                var set = p[0];
                parameters = set.Type.GetProperties().Where(p => p.Name.StartsWith("Tb")).Select((p,i) => Expression.Parameter(p.PropertyType,$"p{i}")).ToArray();
                lambdaTypeSet = p[0];
                var body = Visit(exp.Body);
                return Expression.Lambda(body, parameters);
            }
            return null;
        }
        /// <summary>
        /// 将属性访问的节点替换掉
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        protected override Expression VisitMember(MemberExpression node)
        {
            int widx;
            // w.Tbxxx
            if (node.Expression?.NodeType == ExpressionType.MemberAccess)
            {
                var parent = node.Expression as MemberExpression;
                if (parent?.Expression?.NodeType == ExpressionType.Parameter &&
                    parent.Expression.Type.Name.StartsWith("TypeSet`") == true &&
                    parent.Expression == lambdaTypeSet &&
                    int.TryParse(parent.Member.Name.Replace("Tb", ""), out widx) && widx > 0)
                {
                    if (parameters[widx - 1].Type != parent.Type) //解决 BaseEntity + AsTable 时报错
                        parameters[widx - 1] = Expression.Parameter(parent.Type, parameters[widx - 1].Name);
                    return Expression.Property(parameters[widx - 1], node.Member.Name);
                }
            }
            // w
            if (node.Expression?.NodeType == ExpressionType.Parameter &&
                node.Expression.Type.Name.StartsWith("TypeSet`") == true &&
                node.Expression == lambdaTypeSet &&
                int.TryParse(node.Member.Name.Replace("Tb", ""), out widx) && widx > 0)
            {
                if (parameters[widx - 1].Type != node.Type) //解决 BaseEntity + AsTable 时报错
                    parameters[widx - 1] = Expression.Parameter(node.Type, parameters[widx - 1].Name);
                return parameters[widx - 1];
            }
            return base.VisitMember(node);
        }
    }
}

using MDbContext.Extension;
using MDbEntity.Attributes;
using System.Linq.Expressions;
using System.Reflection;

namespace MDbContext.NewExpSql.ExpressionVisitor
{
    internal class ParameterExpVisitor : BaseVisitor<ParameterExpression>
    {
        public override void DoVisit(ParameterExpression exp, SqlConfig config, SqlContext context)
        {
            var t = exp.Type;
            var props = t.GetProperties();
            var alia = context.GetTableAlias(t.Name);
            foreach (PropertyInfo item in props)
            {
                if (item.GetAttribute<IgnoreAttribute>() != null)
                    continue;
                var col = item.GetAttribute<ColumnNameAttribute>();
                if (config.RequiredColumnAlias && col != null)
                    context += ($"{alia}.{col.Name} {item.Name},");
                else
                    context += ($"{alia}.{item.Name},");

            }
        }
    }
}

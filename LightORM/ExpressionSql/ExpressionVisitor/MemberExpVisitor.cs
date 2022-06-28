using MDbContext.ExpSql.Extension;
using MDbContext.Extension;
using MDbContext.Utils;
using MDbEntity.Attributes;
using System;
using System.Linq.Expressions;

namespace MDbContext.ExpressionSql.ExpressionVisitor;

internal class MemberExpVisitor : BaseVisitor<MemberExpression>
{
    public override void DoVisit(MemberExpression exp, SqlConfig config, SqlContext context)
    {
        if (exp.Type.IsClass && exp.Type != typeof(string))
        {
            // 更新实体/插入实体
            ResolveEntity(exp, config, context);
            return;
        }
        //resolve member name
        if ((config.RequiredValue && config.BinaryPosition == BinaryPosition.Right) || exp.Expression.NodeType != ExpressionType.Parameter)
        {
            //resolve value
            var v = Expression.Lambda(exp).Compile().DynamicInvoke();
            context.AppendDbParameter(v);
        }
        else
        {
            var col = exp.Member.GetColumnName(context, config);
            context.Append(col);
            context.AddFieldName(col);
        }
    }

    private void ResolveEntity(MemberExpression exp, SqlConfig config, SqlContext context)
    {
        var e = Expression.Lambda(exp).Compile().DynamicInvoke();
        var eType = e.GetType();
        var props = eType.GetProperties();
        for (int i = 0; i < props.Length; i++)
        {
            var p = props[i];
            object value = e.AccessValue(exp.Type, p.Name);//p.GetValue(e, null);//
            if (value == null || value == default || value == DBNull.Value)
                continue;
            if (p.GetAttribute<IgnoreAttribute>() != null)
                continue;
            var name = context.GetColumn(p.Name).FieldName!;
            context.AddEntityField(name, value);
        }
    }
}

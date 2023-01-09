using MDbContext.ExpSql.Extension;
using MDbContext.Extension;
using MDbContext.Utils;
using MDbEntity.Attributes;
using System;
using System.Collections;
using System.Linq.Expressions;

namespace MDbContext.ExpressionSql.ExpressionVisitor;

internal class MemberExpVisitor : BaseVisitor<MemberExpression>
{
    public override void DoVisit(MemberExpression exp, SqlConfig config, SqlContext context)
    {
        if (config.RequiredResolveEntity)
        {
            // 更新实体/插入实体
            ResolveEntity(exp, config, context);
            return;
        }
        //resolve member name
        if (config.RequiredValue)
        {
            //resolve value
            var v = Expression.Lambda(exp).Compile().DynamicInvoke();
            // string特殊处理，否则会进入IEnumerable分支
            if (v is string str)
            {
                context.AppendDbParameter(str);
            }
            else if (v is IEnumerable array)
            {
                foreach (var item in array)
                {
                    //context += $"'{item}', ";
                    context.AppendDbParameter(item);
                    context += ", ";
                }
                context -= ", ";
            }
            else
            {
                context.AppendDbParameter(v);
            }
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
            if (p.HasAttribute<IgnoreAttribute>())
                continue;

            object value = e.AccessValue(exp.Type, p.Name);//p.GetValue(e, null);//
            if (value == null || value == default || value == DBNull.Value)
                continue;
            var name = context.GetColumn(eType, p.Name).FieldName!;
            context.AddEntityField(name, value);
        }
    }
}

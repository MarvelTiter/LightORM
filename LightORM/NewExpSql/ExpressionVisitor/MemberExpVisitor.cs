﻿using MDbContext.ExpSql.Extension;
using MDbContext.Extension;
using MDbContext.Utils;
using MDbEntity.Attributes;
using System;
using System.Linq.Expressions;

namespace MDbContext.NewExpSql.ExpressionVisitor
{
    internal class MemberExpVisitor : BaseVisitor<MemberExpression>
    {
        public override void DoVisit(MemberExpression exp, SqlConfig config, ISqlContext context)
        {
            if (exp.Type.IsClass && exp.Type != typeof(string))
            {
                // 更新实体/插入实体
                ResolveEntity(exp, config, context);
                return;
            }
            //resolve member name
            var col = exp.Member.GetColumnName(context, config.RequiredColumnAlias, config.RequiredTableAlias);
            if (config.RequiredValue && config.BinaryPosition == BinaryPosition.Right)
            {
                //resolve value
                var v = Expression.Lambda(exp).Compile().DynamicInvoke();
                context.AppendDbParameter(v);
            }
            else
            {
                context.Append(col);
            }
        }

        private void ResolveEntity(MemberExpression exp, SqlConfig config, ISqlContext context)
        {
            var e = Expression.Lambda(exp).Compile().DynamicInvoke();
            var eType = e.GetType();
            var props = eType.GetProperties();
            for (int i = 0; i < props.Length; i++)
            {
                var p = props[i];
                object value = e.AccessValue(exp.Type, p.Name);
                if (value == null || value == default || value == DBNull.Value)
                    continue;
                if (p.GetAttribute<IgnoreAttribute>() != null)
                    continue;
                if (p.GetAttribute<PrimaryKeyAttribute>() != null)
                    continue;
                var name = p.GetAttribute<ColumnNameAttribute>()?.Name ?? p.Name;
                context.AddEntityField(name, value);
            }
        }
    }
}
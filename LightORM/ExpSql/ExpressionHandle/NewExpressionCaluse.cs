﻿using MDbEntity.Attributes;
using Shared.ExpSql.Extension;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace DExpSql.ExpressionHandle
{
    class NewExpressionCaluse : BaseExpressionSql<NewExpression>
    {
        protected override SqlCaluse Select(NewExpression exp, SqlCaluse sqlCaluse)
        {
            var len = exp.Arguments.Count;
            for (int i = 0; i < len; i++)
            {
                var argExp = exp.Arguments[i];
                sqlCaluse.GetMemberName = () =>
                {
                    return exp.Members[i].Name;
                };
                ExpressionVisit.Select(argExp, sqlCaluse);
            }
            return sqlCaluse;
        }

        protected override SqlCaluse SelectMethod(NewExpression exp, SqlCaluse sqlCaluse)
        {
            for (int i = 0; i < exp.Arguments.Count; i++)
            {
                var argExp = exp.Arguments[i];
                ExpressionVisit.SelectMethod(argExp, sqlCaluse);
            }
            return sqlCaluse;
        }

        protected override SqlCaluse GroupBy(NewExpression exp, SqlCaluse sqlCaluse)
        {
            for (int i = 0; i < exp.Arguments.Count; i++)
            {
                var argExp = exp.Arguments[i];
                ExpressionVisit.GroupBy(argExp, sqlCaluse);
            }
            return sqlCaluse;
        }

        protected override SqlCaluse PrimaryKey(NewExpression exp, SqlCaluse sqlCaluse)
        {
            for (int i = 0; i < exp.Members.Count; i++)
            {
                var name = exp.Members[i].Name;
                if (!sqlCaluse.IgnoreFields.Contains(name))
                    sqlCaluse.IgnoreFields.Add(name);
            }
            return sqlCaluse;
        }
        protected override SqlCaluse Update(NewExpression exp, SqlCaluse sqlCaluse)
        {
            for (int i = 0; i < exp.Members.Count; i++)
            {
                var member = exp.Members[i];
                var arg = exp.Arguments[i];
                //var name = member.GetAttribute<ColumnNameAttribute>()?.Name ?? member.Name;
                var func = Expression.Lambda(arg).Compile();
                var value = func.DynamicInvoke();
                if (value == null || value == DBNull.Value)
                    continue;
                sqlCaluse += $" {member.Name} = ";
                sqlCaluse += sqlCaluse.AddDbParameter(value);
                sqlCaluse += ",\n";
            }
            if (sqlCaluse.EndWith(",\n"))
            {
                sqlCaluse.Sql.Remove(sqlCaluse.Sql.Length - 2, 2);
            }
            return sqlCaluse;
        }

        protected override SqlCaluse Insert(NewExpression exp, SqlCaluse sqlCaluse)
        {
            for (int i = 0; i < exp.Members.Count; i++)
            {
                var member = exp.Members[i];
                var name = member.GetAttribute<ColumnNameAttribute>()?.Name ?? member.Name;
                sqlCaluse.SelectFields.Add(name);
                ExpressionVisit.Insert(exp.Arguments[i], sqlCaluse);
            }
            return sqlCaluse;
        }
    }
}

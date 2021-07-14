using MDbContext.Extension;
using MDbEntity.Attributes;
using System;
using System.Linq.Expressions;

namespace DExpSql.ExpressionHandle {
    class NewExpressionCaluse : BaseExpressionSql<NewExpression> {
        protected override SqlCaluse Select(NewExpression exp, SqlCaluse sqlCaluse) {
            for (int i = 0; i < exp.Arguments.Count; i++) {
                var argExp = exp.Arguments[i];
                if (sqlCaluse.GetMemberName == null)
                    sqlCaluse.GetMemberName = () => {
                        return exp.Members[i].Name;
                    };
                ExpressionVisit.Select(argExp, sqlCaluse);
            }
            return sqlCaluse;
        }

        protected override SqlCaluse GroupBy(NewExpression exp, SqlCaluse sqlCaluse) {
            for (int i = 0; i < exp.Arguments.Count; i++) {
                var argExp = exp.Arguments[i];
                ExpressionVisit.GroupBy(argExp, sqlCaluse);
            }
            return sqlCaluse;
        }

        protected override SqlCaluse PrimaryKey(NewExpression exp, SqlCaluse sqlCaluse) {
            for (int i = 0; i < exp.Members.Count; i++) {
                var name = exp.Members[i].Name;
                if (!sqlCaluse.IgnoreFields.Contains(name))
                    sqlCaluse.IgnoreFields.Add(name);
            }
            return sqlCaluse;
        }
        protected override SqlCaluse Update(NewExpression exp, SqlCaluse sqlCaluse) {
            for (int i = 0; i < exp.Members.Count; i++) {
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
            if (sqlCaluse.EndWith(",\n")) {
                sqlCaluse.Sql.Remove(sqlCaluse.Sql.Length - 2, 2);
            }
            return sqlCaluse;
        }

        protected override SqlCaluse Insert(NewExpression exp, SqlCaluse sqlCaluse) {
            for (int i = 0; i < exp.Members.Count; i++) {
                var member = exp.Members[i];
                var name = member.GetAttribute<ColumnNameAttribute>()?.Name ?? member.Name;
                sqlCaluse.SelectFields.Add(name);
                ExpressionVisit.Insert(exp.Arguments[i], sqlCaluse);
            }
            return sqlCaluse;
        }
    }
}

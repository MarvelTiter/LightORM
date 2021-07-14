using DExpSql;
using MDbContext.Extension;
using MDbEntity.Attributes;
using System.Linq.Expressions;

namespace ExpSql.ExpressionHandle {
    class MemberInitExpressionCaluse : BaseExpressionSql<MemberInitExpression> {
        protected override SqlCaluse Update(MemberInitExpression exp, SqlCaluse sqlCaluse) {
            var bindings = exp.Bindings;
            foreach (MemberBinding binding in bindings) {
                if (binding is MemberAssignment memberExp) {
                    var name = binding.Member.GetAttribute<ColumnNameAttribute>()?.Name;
                    var value = Expression.Lambda(memberExp.Expression).Compile().DynamicInvoke();
                    sqlCaluse += $" {name ?? binding.Member.Name} = ";
                    sqlCaluse += sqlCaluse.AddDbParameter(value);
                    sqlCaluse += ",\n";
                }
            }
            if (sqlCaluse.EndWith(",\n")) {
                sqlCaluse.Sql.Remove(sqlCaluse.Sql.Length - 2, 2);
            }
            return sqlCaluse;
        }
    }
}

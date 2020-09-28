using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace DExpSql.ExpressionHandle
{
    class BinaryExpressionCaluse : BaseExpressionSql<BinaryExpression>
    {
		private void OperatorParser(ExpressionType expressionNodeType, int operatorIndex, SqlCaluse sqlCaluse, bool useIs = false)
		{
			switch (expressionNodeType)
			{
				case ExpressionType.And:
				case ExpressionType.AndAlso:
					sqlCaluse.Sql.Insert(operatorIndex, "\n AND");
					break;
				case ExpressionType.Equal:
					if (useIs)
					{
						sqlCaluse.Sql.Insert(operatorIndex, " IS");
					}
					else
					{
						sqlCaluse.Sql.Insert(operatorIndex, " =");
					}
					break;
				case ExpressionType.GreaterThan:
					sqlCaluse.Sql.Insert(operatorIndex, " >");
					break;
				case ExpressionType.GreaterThanOrEqual:
					sqlCaluse.Sql.Insert(operatorIndex, " >=");
					break;
				case ExpressionType.NotEqual:
					if (useIs)
					{
						sqlCaluse.Sql.Insert(operatorIndex, " IS NOT");
					}
					else
					{
						sqlCaluse.Sql.Insert(operatorIndex, " <>");
					}
					break;
				case ExpressionType.Or:
				case ExpressionType.OrElse:
					sqlCaluse.Sql.Insert(operatorIndex, "\n OR");
					break;
				case ExpressionType.LessThan:
					sqlCaluse.Sql.Insert(operatorIndex, " <");
					break;
				case ExpressionType.LessThanOrEqual:
					sqlCaluse.Sql.Insert(operatorIndex, " <=");
					break;
				default:
					throw new NotImplementedException("未实现的节点类型" + expressionNodeType);
			}
		}

		protected override SqlCaluse Where(BinaryExpression exp, SqlCaluse sqlCaluse)
        {
            ExpressionVisit.Where(exp.Left, sqlCaluse);
			var insertIndex = sqlCaluse.Length;

			ExpressionVisit.Where(exp.Right, sqlCaluse);
			var endIndex = sqlCaluse.Length;
			var b = endIndex - insertIndex == 5 && sqlCaluse.EndWith("null");
			OperatorParser(exp.NodeType, insertIndex, sqlCaluse, b);
            return sqlCaluse;
        }

        protected override SqlCaluse Join(BinaryExpression exp, SqlCaluse sqlCaluse)
        {
			sqlCaluse += " ON ";
			ExpressionVisit.Join(exp.Left, sqlCaluse);
			var insertIndex = sqlCaluse.Length;

			ExpressionVisit.Join(exp.Right, sqlCaluse);
			OperatorParser(exp.NodeType, insertIndex, sqlCaluse, false);

			return sqlCaluse;
        }
    }
}

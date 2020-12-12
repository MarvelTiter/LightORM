using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace DExpSql.ExpressionHandle {
    internal class MethodCallExpressionCaluse : BaseExpressionSql<MethodCallExpression> {
        Dictionary<string, Func<MethodCallExpression, SqlCaluse, SqlCaluse>> methodDic = new Dictionary<string, Func<MethodCallExpression, SqlCaluse, SqlCaluse>>()
        {
            {"Like",LikeMethod },
            {"LeftLike",LeftLikeMethod },
            {"RightLike",RightLikeMethod },
            {"In",InMethod },
            {"Sum",SelectSum },
            {"Count",SelectCount },
        };

        protected override SqlCaluse Where(MethodCallExpression exp, SqlCaluse sqlCaluse) {
            var key = exp.Method.Name;
            if (methodDic.ContainsKey(key)) {
                var func = methodDic[key];
                func.Invoke(exp, sqlCaluse);
            }
            return sqlCaluse;
        }

        protected override SqlCaluse Select(MethodCallExpression exp, SqlCaluse sqlCaluse) {
            var key = exp.Method.Name;
            if (methodDic.ContainsKey(key)) {
                var func = methodDic[key];
                func.Invoke(exp, sqlCaluse);
            }
            return sqlCaluse;
        }

        private static SqlCaluse SelectSum(MethodCallExpression exp, SqlCaluse sqlCaluse) {
            sqlCaluse.SelectMethod.Append("\n SUM(CASE WHEN");
            var a = exp.Arguments[0];
            ExpressionVisit.SelectMethod(a, sqlCaluse);
            sqlCaluse.SelectMethod.Append(" THEN 1 ELSE 0 END) ");
            sqlCaluse.SelectMethod.Append(sqlCaluse.GetMemberName());
            sqlCaluse.SelectFields.Add(sqlCaluse.SelectMethod.ToString());
            sqlCaluse.SelectMethod.Clear();
            return sqlCaluse;
        }

        private static SqlCaluse SelectCount(MethodCallExpression exp, SqlCaluse sqlCaluse) {
            sqlCaluse.SelectMethod.Append("\n COUNT(CASE WHEN");
            var a = exp.Arguments[0];
            ExpressionVisit.SelectMethod(a, sqlCaluse);
            sqlCaluse.SelectMethod.Append(" THEN 1 ELSE null END) ");
            sqlCaluse.SelectMethod.Append(sqlCaluse.GetMemberName());
            sqlCaluse.SelectFields.Add(sqlCaluse.SelectMethod.ToString());
            sqlCaluse.SelectMethod.Clear();
            return sqlCaluse;
        }

        private static SqlCaluse InMethod(MethodCallExpression exp, SqlCaluse sqlCaluse) {
            ExpressionVisit.Where(exp.Arguments[0], sqlCaluse);
            sqlCaluse += " In";
            ExpressionVisit.Where(exp.Arguments[1], sqlCaluse);
            return sqlCaluse;
        }

        private static SqlCaluse RightLikeMethod(MethodCallExpression exp, SqlCaluse sqlCaluse) {
            ExpressionVisit.Where(exp.Arguments[0], sqlCaluse);
            sqlCaluse += " Like";
            sqlCaluse.LikeMode = 3;
            ExpressionVisit.Where(exp.Arguments[1], sqlCaluse);
            return sqlCaluse;
        }

        private static SqlCaluse LeftLikeMethod(MethodCallExpression exp, SqlCaluse sqlCaluse) {
            ExpressionVisit.Where(exp.Arguments[0], sqlCaluse);
            sqlCaluse += " Like";
            sqlCaluse.LikeMode = 2;
            ExpressionVisit.Where(exp.Arguments[1], sqlCaluse);
            return sqlCaluse;
        }

        private static SqlCaluse LikeMethod(MethodCallExpression exp, SqlCaluse sqlCaluse) {
            ExpressionVisit.Where(exp.Arguments[0], sqlCaluse);
            sqlCaluse += " Like";
            sqlCaluse.LikeMode = 1;
            ExpressionVisit.Where(exp.Arguments[1], sqlCaluse);
            return sqlCaluse;
        }
    }
}

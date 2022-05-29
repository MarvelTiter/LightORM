using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace DExpSql.ExpressionHandle
{
    internal class MethodCallExpressionCaluse : BaseExpressionSql<MethodCallExpression>
    {
        Dictionary<string, Func<MethodCallExpression, SqlCaluse, SqlCaluse>> methodDic = new Dictionary<string, Func<MethodCallExpression, SqlCaluse, SqlCaluse>>()
        {
            {"Like",LikeMethod },
            {"NotLike",NotLikeMethod },
            {"LeftLike",LeftLikeMethod },
            {"RightLike",RightLikeMethod },
            {"In",InMethod },
            {"Sum",SelectSum },
            {"Count",SelectCount },
            {"GroupConcat",SelectGroupConcat },
            {"Coalesce",SelectCoalesce }
        };



        protected override SqlCaluse Where(MethodCallExpression exp, SqlCaluse sqlCaluse)
        {
            var key = exp.Method.Name;
            if (methodDic.ContainsKey(key))
            {
                var func = methodDic[key];
                func.Invoke(exp, sqlCaluse);
            }
            return sqlCaluse;
        }

        protected override SqlCaluse Join(MethodCallExpression exp, SqlCaluse sqlCaluse)
        {
            var key = exp.Method.Name;
            sqlCaluse += " ON ";
            if (methodDic.ContainsKey(key))
            {
                var func = methodDic[key];
                func.Invoke(exp, sqlCaluse);
            }
            return sqlCaluse;
        }

        protected override SqlCaluse Select(MethodCallExpression exp, SqlCaluse sqlCaluse)
        {
            var key = exp.Method.Name;
            if (methodDic.ContainsKey(key))
            {
                var func = methodDic[key];
                func.Invoke(exp, sqlCaluse);
            }
            return sqlCaluse;
        }

        private static SqlCaluse SelectSum(MethodCallExpression exp, SqlCaluse sqlCaluse)
        {
            var a = exp.Arguments[0];
            ExpressionVisit.SelectMethod(a, sqlCaluse);
            if (sqlCaluse.SelectMethodType == 0)
            {
                AddColumn(sqlCaluse, "\n SUM(CASE WHEN{0} THEN 1 ELSE 0 END) ");
            }
            else
            {
                AddColumn(sqlCaluse, "\n SUM({0}) ");
            }
            return sqlCaluse;
        }

        private static SqlCaluse SelectCount(MethodCallExpression exp, SqlCaluse sqlCaluse)
        {
            if (exp.Arguments.Count == 0)
            {
                AddColumn(sqlCaluse, "\n COUNT(*) ");
            }
            else
            {
                var a = exp.Arguments[0];
                ExpressionVisit.SelectMethod(a, sqlCaluse);
                if (sqlCaluse.SelectMethodType == 0)
                {
                    AddColumn(sqlCaluse, "\n COUNT(CASE WHEN{0} THEN 1 ELSE null END) ");
                }
                else
                {
                    AddColumn(sqlCaluse, "\n COUNT({0}) ");
                }
            }
            return sqlCaluse;
        }

        private static SqlCaluse SelectGroupConcat(MethodCallExpression exp, SqlCaluse sqlCaluse)
        {
            if (sqlCaluse.DbType == MDbContext.DbBaseType.MySql)
            {
                var a = exp.Arguments[0];
                ExpressionVisit.SelectMethod(a, sqlCaluse);
                AddColumn(sqlCaluse, "\n GROUP_CONCAT({0}) ");
            }
            else
            {
                throw new Exception("GroupConcat 只支持 MySql");
            }
            return sqlCaluse;
        }

        private static SqlCaluse SelectCoalesce(MethodCallExpression exp, SqlCaluse sqlCaluse)
        {
            var arg = exp.Arguments[0];
            ExpressionVisit.SelectMethod(arg, sqlCaluse);
            var finalName = "Coalesce";
            if (exp.Arguments.Count > 1)
            {
                finalName = ((ConstantExpression)exp.Arguments[1]).Value.ToString();
            }
            var fields = sqlCaluse.SelectMethod.ToString().Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            var coalParams = new List<string>();
            foreach (var field in fields)
            {
                coalParams.Add(sqlCaluse.DbHelper.ToDbString(field));
            }
            var col = $"COALESCE({string.Join(",", coalParams)},'{finalName}') {sqlCaluse.GetMemberName()}";
            sqlCaluse.SelectFields.Add(col);
            sqlCaluse.SelectMethod.Clear();
            return sqlCaluse;
        }

        private static void AddColumn(SqlCaluse sqlCaluse, string columnTemplate)
        {
            var content = sqlCaluse.SelectMethod.ToString();
            var col = string.Format(columnTemplate, content) + sqlCaluse.GetMemberName();
            sqlCaluse.SelectFields.Add(col);
            sqlCaluse.SelectMethod.Clear();
            sqlCaluse.SelectMethodType = 1;
        }

        private static SqlCaluse InMethod(MethodCallExpression exp, SqlCaluse sqlCaluse)
        {
            ExpressionVisit.Join(exp.Arguments[0], sqlCaluse);
            sqlCaluse += " In (";
            ExpressionVisit.In(exp.Arguments[1], sqlCaluse);
            sqlCaluse += ")";
            return sqlCaluse;
        }

        private static SqlCaluse RightLikeMethod(MethodCallExpression exp, SqlCaluse sqlCaluse)
        {
            ExpressionVisit.Where(exp.Arguments[0], sqlCaluse);
            sqlCaluse += " Like ";
            sqlCaluse.LikeMode = 3;
            ExpressionVisit.Where(exp.Arguments[1], sqlCaluse);
            return sqlCaluse;
        }

        private static SqlCaluse LeftLikeMethod(MethodCallExpression exp, SqlCaluse sqlCaluse)
        {
            ExpressionVisit.Where(exp.Arguments[0], sqlCaluse);
            sqlCaluse += " Like ";
            sqlCaluse.LikeMode = 2;
            ExpressionVisit.Where(exp.Arguments[1], sqlCaluse);
            return sqlCaluse;
        }

        private static SqlCaluse LikeMethod(MethodCallExpression exp, SqlCaluse sqlCaluse)
        {
            ExpressionVisit.Where(exp.Arguments[0], sqlCaluse);
            sqlCaluse += " Like ";
            sqlCaluse.LikeMode = 1;
            ExpressionVisit.Where(exp.Arguments[1], sqlCaluse);
            return sqlCaluse;
        }
        private static SqlCaluse NotLikeMethod(MethodCallExpression exp, SqlCaluse sqlCaluse)
        {
            ExpressionVisit.Where(exp.Arguments[0], sqlCaluse);
            sqlCaluse += " Not Like ";
            sqlCaluse.LikeMode = 1;
            ExpressionVisit.Where(exp.Arguments[1], sqlCaluse);
            return sqlCaluse;
        }
    }
}

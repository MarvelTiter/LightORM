using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace MDbContext.ExpressionSql.ExpressionVisitor;

internal class MethodCallExpVisitor : BaseVisitor<MethodCallExpression>
{
    Dictionary<string, Action<MethodCallExpression, SqlConfig, SqlContext>> methodDic = new Dictionary<string, Action<MethodCallExpression, SqlConfig, SqlContext>>()
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
    public override void DoVisit(MethodCallExpression exp, SqlConfig config, SqlContext context)
    {
        var key = exp.Method.Name;
        if (methodDic.TryGetValue(key, out var func))
        {
            func.Invoke(exp, config, context);
        }
    }

    public static void LikeMethod(MethodCallExpression exp, SqlConfig config, SqlContext context)
    {
        ExpressionVisit.Visit(exp.Arguments[0], config, context);
        context += " LIKE ";
        context.LikeMode = 1;
        ExpressionVisit.Visit(exp.Arguments[1], config, context);
    }
    public static void NotLikeMethod(MethodCallExpression exp, SqlConfig config, SqlContext context)
    {
        ExpressionVisit.Visit(exp.Arguments[0], config, context);
        context += " NOT LIKE ";
        context.LikeMode = 1;
        ExpressionVisit.Visit(exp.Arguments[1], config, context);
    }
    public static void LeftLikeMethod(MethodCallExpression exp, SqlConfig config, SqlContext context)
    {
        ExpressionVisit.Visit(exp.Arguments[0], config, context);
        context += " LIKE ";
        context.LikeMode = 2;
        ExpressionVisit.Visit(exp.Arguments[1], config, context);
    }
    public static void RightLikeMethod(MethodCallExpression exp, SqlConfig config, SqlContext context)
    {
        ExpressionVisit.Visit(exp.Arguments[0], config, context);
        context += " LIKE ";
        context.LikeMode = 3;
        ExpressionVisit.Visit(exp.Arguments[1], config, context);
    }
    public static void InMethod(MethodCallExpression exp, SqlConfig config, SqlContext context)
    {
        // Where 条件中 In
        if (config.SqlType != SqlPartial.Where) throw new InvalidOperationException("In函数仅能用于Where子句中");
        ExpressionVisit.Visit(exp.Arguments[0], config, context);
        context.Append(" IN (");
        config.BinaryPosition = BinaryPosition.Right;
        ExpressionVisit.Visit(exp.Arguments[1], config, context);
        context.Append(")");
    }
    public static void SelectSum(MethodCallExpression exp, SqlConfig config, SqlContext context)
    {
        var a = exp.Arguments[0];
        var current = context.GetCurrentFragment();
        context.SetFragment(new SqlFragment());
        ExpressionVisit.Visit(a, SqlConfig.SelectFunc, context);
        var temp = context.GetCurrentFragment();
        var newLine = current.Length > 0 ? "\n" : "";
        if (a is UnaryExpression)
        {
            current.Append($"{newLine}SUM(CASE WHEN {temp} THEN 1 ELSE 0 END) ");
        }
        else
        {
            current.Append($"{newLine}SUM({temp}) ");
        }
        context.SetFragment(current);
    }
    public static void SelectCount(MethodCallExpression exp, SqlConfig config, SqlContext context)
    {
        if (exp.Arguments.Count == 0)
        {
            context.Append("COUNT(*) ");
        }
        else
        {
            var a = exp.Arguments[0];
            var current = context.GetCurrentFragment();
            context.SetFragment(new SqlFragment());
            ExpressionVisit.Visit(a, SqlConfig.SelectFunc, context);
            var temp = context.GetCurrentFragment();
            var newLine = current.Length > 0 ? "\n" : "";
            if (a is UnaryExpression)
            {
                current.Append($"{newLine}COUNT(CASE WHEN {temp} THEN 1 ELSE null END) ");
            }
            else
            {
                current.Append($"{newLine}COUNT({temp}) ");
            }
            context.SetFragment(current);
        }
    }
    public static void SelectGroupConcat(MethodCallExpression exp, SqlConfig config, SqlContext context)
    {

    }
    public static void SelectCoalesce(MethodCallExpression exp, SqlConfig config, SqlContext context)
    {
        var arg = exp.Arguments[0];
        var current = context.GetCurrentFragment();
        context.SetFragment(new SqlFragment());
        ExpressionVisit.Visit(arg, SqlConfig.SelectFunc, context);
        var temp = context.GetCurrentFragment();
        var finalName = "Coalesce";
        if (exp.Arguments.Count > 1)
        {
            finalName = ((ConstantExpression)exp.Arguments[1]!).Value!.ToString();
        }
        var fields = temp.Sql.ToString().Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
        var coalParams = new List<string>();
        foreach (var field in fields)
        {
            coalParams.Add(context.DbHandler.DbStringConvert(field));
        }
        var newLine = current.Length > 0 ? "\n" : "";
        current.Append($"{newLine}COALESCE({string.Join(",", coalParams)},'{finalName}')");
        context.SetFragment(current);
    }
}

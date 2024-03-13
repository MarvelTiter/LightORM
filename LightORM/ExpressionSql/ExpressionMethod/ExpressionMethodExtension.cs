using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace LightORM.ExpressionSql.ExpressionMethod;

public static class SqlFn
{
    public static bool Like(this string self, string keyWord)
    {
        return true;
    }

    public static bool NotLike(this string self, string keyWord)
    {
        return true;
    }

    public static bool LeftLike(this string self, string keyWord)
    {
        return true;
    }

    public static bool RightLike(this string self, string keyWord)
    {
        return true;
    }

    public static bool In(this string self, params object[] array)
    {
        return true;
    }

    public static bool In<T>(this T self, params object[] array) where T : struct
    {
        return true;
    }

    //public static int Sum(Expression<Func<bool>> exp)
    //{//this object self,
    //    return 0;
    //}

    public static int Sum(Expression<Func<object>> exp)
    {
        return 0;
    }

    //public static int Count(Expression<Func<bool>> exp)
    //{
    //    return 0;
    //}

    public static int Count(Expression<Func<object>> exp)
    {
        return 0;
    }
    public static int Count()
    {
        return 0;
    }

    public static string Coalesce(Expression<Func<object>> exp)
    {
        return "Coalesce";
    }

    public static string Coalesce(Expression<Func<object>> exp, string p)
    {
        return p;
    }

    public static int GroupConcat(Expression<Func<object>> exp)
    {
        return 0;
    }
}
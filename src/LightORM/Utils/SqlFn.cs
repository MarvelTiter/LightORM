using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LightORM;

public class SqlFn
{
    /// <summary>
    /// <para>当T为返回bool的表达式或者三元表达式时，会解析成CASE WHEN语句</para>
    /// <para>否则COUNT(column)</para>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="column"></param>
    /// <returns></returns>
    public static int Count<T>(T column)
    {
        return default;
    }
    /// <summary>
    /// COUNT(*)
    /// </summary>
    /// <returns></returns>
    public static int Count()
    {
        return default;
    }
    /// <summary>
    /// SUM(CASE WHEN exp THEN val ELSE 0 END)
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="exp"></param>
    /// <param name="val"></param>
    /// <returns></returns>
    public static double Sum<T>(bool exp, T val)
    {
        return default!;
    }
    /// <summary>
    /// SUM(val)
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="val"></param>
    /// <returns></returns>
    public static double Sum<T>(T val)
    {
        return default!;
    }
    /// <summary>
    /// AVG(CASE WHEN exp THEN val ELSE 0 END)
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="exp"></param>
    /// <param name="val"></param>
    /// <returns></returns>
    public static double Avg<T>(bool exp, T val)
    {
        return default;
    }
    /// <summary>
    /// AVG(val)
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="val"></param>
    /// <returns></returns>
    public static double Avg<T>(T val)
    {
        return default;
    }

    public static ICaseFragment<T> Case<T>(T? column)
    {
        return default!;
    }
    public static ICaseFragment<T> Case<T>()
    {
        return default!;
    }
    
    /// <summary>
    /// 分组数据中拼接字符串
    /// <para>MySql -> GROUP_CONCAT</para>
    /// <para>Sqlite -> GROUP_CONCAT</para>
    /// <para>Oracle -> LISTAGG</para>
    /// <para>SqlServer 2017 -> STRING_AGG</para>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="column"></param>
    /// <returns></returns>
    public static IGroupJoinFn Join<T>(T? column)
    {
        return default!;
    }

}

public interface IGroupJoinFn
{
    IGroupJoinFn OrderBy<T>(T? column);
    IGroupJoinFn Distinct();
    IGroupJoinFn Separator(string separator);
    string Value ();
}

public interface ICaseFragment<T>
{
    ICaseFragment<T> When(bool condition);
    ICaseFragment<T> Then(T? value);
    ICaseFragment<T> Else(T? value);
    T End();
}

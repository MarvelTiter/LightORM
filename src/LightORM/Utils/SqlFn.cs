using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LightORM;

public static partial class SqlFn
{
    /// <summary>
    /// Column IN ('value1', 'value2', 'value3' ...)
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="column"></param>
    /// <param name="values"></param>
    /// <returns></returns>
    public static bool In<T>(this object? column, params T[] values) => false;
    /// <summary>
    /// 绝对值
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="column"></param>
    /// <returns></returns>
    public static int Abs<T>(T column) => 0;
    /// <summary>
    /// 返回一个数值，舍入到指定的长度或精度。
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="Column"></param>
    /// <param name="length "></param>
    /// <returns></returns>
    public static T Round<T>(T Column, int length) => default!;

    /// <summary>
    /// <para>当T为返回bool的表达式或者三元表达式时，会解析成CASE WHEN语句</para>
    /// <para>否则COUNT(column)</para>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="column"></param>
    /// <returns></returns>
    public static int Count<T>(T column) => 0;
    /// <summary>
    /// <para>当T为返回bool的表达式或者三元表达式时，会解析成CASE WHEN语句</para>
    /// <para>否则COUNT(DISTINCT column)</para>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="column"></param>
    /// <returns></returns>
    public static int CountDistinct<T>(T column) => 0;
    /// <summary>
    /// COUNT(*)
    /// </summary>
    /// <returns></returns>
    public static int Count() => 0;
    /// <summary>
    /// SUM(CASE WHEN exp THEN val ELSE 0 END)
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="exp"></param>
    /// <param name="val"></param>
    /// <returns></returns>
    public static double Sum<T>(bool exp, T val) => 0;
    /// <summary>
    /// SUM(val)
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="val"></param>
    /// <returns></returns>
    public static double Sum<T>(T val) => 0;
    /// <summary>
    /// AVG(CASE WHEN exp THEN val ELSE 0 END)
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="exp"></param>
    /// <param name="val"></param>
    /// <returns></returns>
    public static double Avg<T>(bool exp, T val) => 0;
    /// <summary>
    /// AVG(val)
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="val"></param>
    /// <returns></returns>
    public static double Avg<T>(T val) => 0;

    public static ICaseFragment<T> Case<T>(T? column) => default!;
    public static ICaseFragment<T> Case<T>() => default!;

    /// <summary>
    /// 分组数据中拼接字符串, 若要条件拼接，需要使用三元表达式
    /// <para>MySql -> GROUP_CONCAT</para>
    /// <para>Sqlite -> GROUP_CONCAT</para>
    /// <para>Oracle -> LISTAGG</para>
    /// <para>SqlServer 2017 -> STRING_AGG</para>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="column"></param>
    /// <returns></returns>
    public static IGroupJoinFn Join<T>(T? column) => default!;
}

partial class SqlFn
{
    [Obsolete("Use NullThen")]
    public static T Nvl<T>(T column, T value) => default!;
    /// <summary>
    /// 检查是否为null，如果是，返回value，在不同的数据库有不同的实现。
    /// eg: 
    /// <para>MySql -> IFNULL</para>
    /// <para>SqlServer -> ISNULL</para>
    /// <para>Oracle -> NVL</para>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="column"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public static T NullThen<T>(T column, T value) => default!;
    [Obsolete("Use NullThen")]
    public static T IsNull<T>(T column, T value) => default!;
}

public interface IGroupJoinFn
{
    IGroupJoinFn OrderBy<T>(T? column);
    IGroupJoinFn Distinct();
    IGroupJoinFn Separator(string separator);
    string Value();
}

public interface ICaseFragment<T>
{
    ICaseFragment<T> When(bool condition);
    ICaseFragment<T> Then(T? value);
    ICaseFragment<T> Else(T? value);
    T End();
}

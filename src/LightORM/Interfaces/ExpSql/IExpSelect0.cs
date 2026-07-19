using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Threading;

namespace LightORM;

public interface IExpSelect0<out TSelect, T1> : IExpSelect where TSelect : IExpSelect
{
    #region 日志输出辅助

    TSelect TagWith(string tag);
    TSelect TagWithCallSite(string tag, [CallerFilePath] string? filePath = null, [CallerMemberName] string? callMember = null, [CallerLineNumber] int? lineNum = null);

    #endregion

    TSelect Count(out long total);
    TSelect Where(Expression<Func<T1, bool>> exp);
    TSelect WhereIf(bool condition, Expression<Func<T1, bool>> exp);
    TSelect Where<TTable>(Expression<Func<TTable, bool>> exp);
    TSelect WhereIf<TTable>(bool condition, Expression<Func<TTable, bool>> exp);
    IEnumerable<T1> ToList();
    T1? First();
    TReturn? First<
#if NET8_0_OR_GREATER
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.PublicProperties)]
#endif
    TReturn>();
    DataTable ToDataTable();
    Task<IList<T1>> ToListAsync(CancellationToken cancellationToken = default);
    IAsyncEnumerable<T1> ToEnumerableAsync(CancellationToken cancellationToken = default);
    Task<T1?> FirstAsync(CancellationToken cancellationToken = default);
    Task<TReturn?> FirstAsync<
#if NET8_0_OR_GREATER
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.PublicProperties)]
#endif
TReturn>(CancellationToken cancellationToken = default);
    Task<DataTable> ToDataTableAsync(CancellationToken cancellationToken = default);
    Task<TMember?> MaxAsync<TMember>(Expression<Func<T1, TMember>> exp, CancellationToken cancellationToken = default);
    Task<TMember?> MinAsync<TMember>(Expression<Func<T1, TMember>> exp, CancellationToken cancellationToken = default);
    Task<double> SumAsync<TMember>(Expression<Func<T1, TMember>> exp, CancellationToken cancellationToken = default);
    Task<int> CountAsync<TMember>(Expression<Func<T1, TMember>> exp, CancellationToken cancellationToken = default);
    Task<int> CountAsync(CancellationToken cancellationToken = default);
    Task<double> AvgAsync<TMember>(Expression<Func<T1, TMember>> exp, CancellationToken cancellationToken = default);
    Task<bool> AnyAsync(CancellationToken cancellationToken = default);
    TSelect Paging(int pageIndex, int pageSize);
    TSelect Skip(int count);
    TSelect Take(int count);
    TMember? Max<TMember>(Expression<Func<T1, TMember>> exp);
    TMember? Min<TMember>(Expression<Func<T1, TMember>> exp);
    double Sum<TMember>(Expression<Func<T1, TMember>> exp);
    int Count<TMember>(Expression<Func<T1, TMember>> exp);
    int Count();
    double Avg<TMember>(Expression<Func<T1, TMember>> exp);

    bool Any();

    //TSelect RollUp();
    TSelect Distinct();
    IExpTemp<T1> AsTemp(string name);
    TSelect Parameterized(bool use = true);

    #region 使用原生sql

    /// <summary>
    /// 共享参数, 跟<see cref="Where(string, object?)"/>, <see cref="WhereIf(bool, string, object?)"/>等搭配使用
    /// </summary>
    /// <param name="parameters"></param>
    /// <returns></returns>
    TSelect WithParameters<
#if NET8_0_OR_GREATER
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)]
#endif
        TParameter>(TParameter parameters);
    TSelect Where<
#if NET8_0_OR_GREATER
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)]
#endif
        TParameter>(string sql, TParameter parameters);
    TSelect WhereIf<
#if NET8_0_OR_GREATER
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)]
#endif
        TParameter>(bool condition, string sql, TParameter parameters);
    TSelect GroupBy<
#if NET8_0_OR_GREATER
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)]
#endif
        TParameter>(string sql, TParameter parameters);
    TSelect Having<
#if NET8_0_OR_GREATER
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)]
#endif
        TParameter>(string sql, TParameter parameters);
    TSelect OrderBy<
#if NET8_0_OR_GREATER
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)]
#endif
        TParameter>(string sql, TParameter parameters);
    TSelect Where(string sql);
    TSelect WhereIf(bool condition, string sql);
    TSelect GroupBy(string sql);
    TSelect Having(string sql);
    TSelect OrderBy(string sql);
    #endregion
}
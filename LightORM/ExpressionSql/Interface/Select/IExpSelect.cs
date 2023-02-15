using MDbContext.ExpressionSql.Providers.Select;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace MDbContext.ExpressionSql.Interface.Select;

public interface IExpSelect0
{
    string ToSql();
    //object GetParameters();
}
public interface IExpSelect0<TSelect, T1> : IExpSelect0 where TSelect : IExpSelect0
{
    TSelect InnerJoin<TAnother>(Expression<Func<TAnother, T1, bool>> exp);
    TSelect LeftJoin<TAnother>(Expression<Func<TAnother, T1, bool>> exp);
    TSelect RightJoin<TAnother>(Expression<Func<TAnother, T1, bool>> exp);
    TSelect InnerJoin<TAnother1, TAnother2>(Expression<Func<TAnother1, TAnother2, bool>> exp);
    TSelect LeftJoin<TAnother1, TAnother2>(Expression<Func<TAnother1, TAnother2, bool>> exp);
    TSelect RightJoin<TAnother1, TAnother2>(Expression<Func<TAnother1, TAnother2, bool>> exp);
    TSelect InnerJoin<TAnother1, TAnother2>(Expression<Func<T1, TAnother1, TAnother2, bool>> exp);
    TSelect LeftJoin<TAnother1, TAnother2>(Expression<Func<T1, TAnother1, TAnother2, bool>> exp);
    TSelect RightJoin<TAnother1, TAnother2>(Expression<Func<T1, TAnother1, TAnother2, bool>> exp);
    TSelect Count(out long total);
    TSelect Where<TAnother>(Expression<Func<TAnother, bool>> exp);
    TSelect WhereIf(bool condition, Expression<Func<T1, bool>> exp);
    TSelect WhereIf<TAnother>(bool condition, Expression<Func<TAnother, bool>> exp);
    //IEnumerable<T1> ToList(Expression<Func<T1, object>> exp);
    IEnumerable<T1> ToList();
    IEnumerable<dynamic> ToDynamicList();
    IEnumerable<TReturn> ToList<TReturn>();
    DataTable ToDataTable();
    #region async
#if NET40
#else
    Task<IList<T1>> ToListAsync();
    Task<IList<dynamic>> ToDynamicListAsync();
    Task<IList<TReturn>> ToListAsync<TReturn>();
    Task<DataTable> ToDataTableAsync();
    Task<TMember> MaxAsync<TMember>(Expression<Func<T1, TMember>> exp);
    Task<double> SumAsync(Expression<Func<T1, object>> exp);
    Task<int> CountAsync(Expression<Func<T1, object>> exp);
    Task<int> CountAsync();
    Task<bool> AnyAsync();
#endif
#endregion
    //int Execute();
    //Task<int> ExecuteAsync();

    TSelect Paging(int pageIndex, int pageSize);
    TMember Max<TMember>(Expression<Func<T1, TMember>> exp);
    double Sum(Expression<Func<T1, object>> exp);
    int Count(Expression<Func<T1, object>> exp);
    int Count();
    bool Any();
   
    TSelect RollUp();
    TSelect Distinct();
    //TSelect From(Func<IExpressionContext, TSelect> sub);
}
public interface IExpSelect<T1> : IExpSelect0<IExpSelect<T1>, T1>
{
    //IEnumerable<TReturn> ToList<TReturn>(Expression<Func<T1, object>> exp);
    //Task<IList<TReturn>> ToListAsync<TReturn>(Expression<Func<T1, object>> exp);
    IExpSelect<T1> OrderBy(Expression<Func<T1, object>> exp, bool asc = true);
    IExpSelect<T1> GroupBy(Expression<Func<T1, object>> exp);
    IExpSelect<T1> Where(Expression<Func<T1, bool>> exp);
}

public interface IExpSelect<T1, T2> : IExpSelect0<IExpSelect<T1, T2>, T1>
{
    //IEnumerable<TReturn> ToList<TReturn>(Expression<Func<T1, T2, object>> exp);
    //IEnumerable<TReturn> ToList<TReturn>(Expression<Func<TypeSet<T1, T2>, object>> exp);
    //Task<IList<TReturn>> ToListAsync<TReturn>(Expression<Func<T1, T2, object>> exp);
    //Task<IList<TReturn>> ToListAsync<TReturn>(Expression<Func<TypeSet<T1, T2>, object>> exp);
    IExpSelect<T1, T2> OrderBy(Expression<Func<T1, T2, object>> exp, bool asc = true);
    //IExpSelect<T1, T2> OrderBy(Expression<Func<TypeSet<T1, T2>, object>> exp, bool asc = true);
    IExpSelect<T1, T2> GroupBy(Expression<Func<T1, T2, object>> exp);
    //IExpSelect<T1, T2> GroupBy(Expression<Func<TypeSet<T1, T2>, object>> exp);
    IExpSelect<T1, T2> Where(Expression<Func<T1, T2, bool>> exp);
    //IExpSelect<T1, T2> Where(Expression<Func<TypeSet<T1, T2>, bool>> exp);
    IExpSelect<T1, T2> InnerJoin<TAnother>(Expression<Func<TypeSet<TAnother, T1, T2>, bool>> exp);
    IExpSelect<T1, T2> LeftJoin<TAnother>(Expression<Func<TypeSet<TAnother, T1, T2>, bool>> exp);
    IExpSelect<T1, T2> RightJoin<TAnother>(Expression<Func<TypeSet<TAnother, T1, T2>, bool>> exp);
}

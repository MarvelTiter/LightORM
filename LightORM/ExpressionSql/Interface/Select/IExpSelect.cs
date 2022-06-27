using MDbContext.ExpressionSql.Providers.Select;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace MDbContext.ExpressionSql.Interface.Select
{
    public interface IExpSelect0 { }
    public interface IExpSelect0<TSelect, T1> : IExpSelect0
    {
        TSelect InnerJoin<TAnother>(Expression<Func<T1, TAnother, bool>> exp);
        TSelect LeftJoin<TAnother>(Expression<Func<T1, TAnother, bool>> exp);
        TSelect RightJoin<TAnother>(Expression<Func<T1, TAnother, bool>> exp);
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
        IEnumerable<T1> ToList(Expression<Func<T1, object>> exp);
        IEnumerable<T1> ToList();
        IEnumerable<TReturn> ToList<TReturn>();
        #region async
        Task<IList<T1>> ToListAsync(Expression<Func<T1, object>> exp);
        Task<IList<T1>> ToListAsync();
        Task<IList<TReturn>> ToListAsync<TReturn>();
        #endregion
        //int Execute();
        //Task<int> ExecuteAsync();
        string ToSql();
        TSelect Paging(int pageIndex, int pageSize);
        TMember Max<TMember>(Expression<Func<T1, TMember>> exp);
        double Sum(Expression<Func<T1, object>> exp);
        int Count(Expression<Func<T1, object>> exp);
    }
    public interface IExpSelect<T1> : IExpSelect0<IExpSelect<T1>, T1>
    {
        IEnumerable<TReturn> ToList<TReturn>(Expression<Func<T1, object>> exp);
        Task<IList<TReturn>> ToListAsync<TReturn>(Expression<Func<T1, object>> exp);
        IExpSelect<T1> OrderBy(Expression<Func<T1, object>> exp, bool asc = true);
        IExpSelect<T1> GroupBy(Expression<Func<T1, object>> exp);
        IExpSelect<T1> Where(Expression<Func<T1, bool>> exp);
    }

    public interface IExpSelect<T1, T2> : IExpSelect0<IExpSelect<T1, T2>, T1>
    {
        IEnumerable<TReturn> ToList<TReturn>(Expression<Func<T1, T2, object>> exp);
        IEnumerable<TReturn> ToList<TReturn>(Expression<Func<TypeSet<T1, T2>, object>> exp);
        Task<IList<TReturn>> ToListAsync<TReturn>(Expression<Func<T1, T2, object>> exp);
        Task<IList<TReturn>> ToListAsync<TReturn>(Expression<Func<TypeSet<T1, T2>, object>> exp);
        IExpSelect<T1, T2> OrderBy(Expression<Func<T1, T2, object>> exp, bool asc = true);
        IExpSelect<T1, T2> OrderBy(Expression<Func<TypeSet<T1, T2>, object>> exp, bool asc = true);
        IExpSelect<T1, T2> GroupBy(Expression<Func<T1, T2, object>> exp);
        IExpSelect<T1, T2> GroupBy(Expression<Func<TypeSet<T1, T2>, object>> exp);
        IExpSelect<T1, T2> Where(Expression<Func<T1, T2, bool>> exp);
        IExpSelect<T1, T2> Where(Expression<Func<TypeSet<T1, T2>, bool>> exp);
    }

    public interface IExpSelect<T1, T2, T3> : IExpSelect0<IExpSelect<T1, T2, T3>, T1>
    {
        IEnumerable<TReturn> ToList<TReturn>(Expression<Func<TypeSet<T1, T2, T3>, object>> exp);
        Task<IList<TReturn>> ToListAsync<TReturn>(Expression<Func<TypeSet<T1, T2, T3>, object>> exp);
        IExpSelect<T1, T2, T3> Where(Expression<Func<TypeSet<T1, T2, T3>, bool>> exp);
        IExpSelect<T1, T2, T3> OrderBy(Expression<Func<TypeSet<T1, T2, T3>, object>> exp, bool asc = true);
        IExpSelect<T1, T2, T3> GroupBy(Expression<Func<TypeSet<T1, T2, T3>, object>> exp);
    }

    public interface IExpSelect<T1, T2, T3, T4> : IExpSelect0<IExpSelect<T1, T2, T3, T4>, T1>
    {
        IEnumerable<TReturn> ToList<TReturn>(Expression<Func<TypeSet<T1, T2, T3, T4>, object>> exp);
        Task<IList<TReturn>> ToListAsync<TReturn>(Expression<Func<TypeSet<T1, T2, T3, T4>, object>> exp);
        IExpSelect<T1, T2, T3, T4> Where(Expression<Func<TypeSet<T1, T2, T3, T4>, bool>> exp);
        IExpSelect<T1, T2, T3, T4> OrderBy(Expression<Func<TypeSet<T1, T2, T3, T4>, object>> exp, bool asc = true);
        IExpSelect<T1, T2, T3, T4> GroupBy(Expression<Func<TypeSet<T1, T2, T3, T4>, object>> exp);
    }

    public interface IExpSelect<T1, T2, T3, T4, T5> : IExpSelect0<IExpSelect<T1, T2, T3, T4, T5>, T1>
    {
        IEnumerable<TReturn> ToList<TReturn>(Expression<Func<TypeSet<T1, T2, T3, T4, T5>, object>> exp);
        Task<IList<TReturn>> ToListAsync<TReturn>(Expression<Func<TypeSet<T1, T2, T3, T4, T5>, object>> exp);
        IExpSelect<T1, T2, T3, T4, T5> Where(Expression<Func<TypeSet<T1, T2, T3, T4, T5>, bool>> exp);
        IExpSelect<T1, T2, T3, T4, T5> OrderBy(Expression<Func<TypeSet<T1, T2, T3, T4, T5>, object>> exp, bool asc = true);
        IExpSelect<T1, T2, T3, T4, T5> GroupBy(Expression<Func<TypeSet<T1, T2, T3, T4, T5>, object>> exp);
    }

    public interface IExpSelect<T1, T2, T3, T4, T5, T6> : IExpSelect0<IExpSelect<T1, T2, T3, T4, T5, T6>, T1>
    {
        IEnumerable<TReturn> ToList<TReturn>(Expression<Func<TypeSet<T1, T2, T3, T4, T5, T6>, object>> exp);
        Task<IList<TReturn>> ToListAsync<TReturn>(Expression<Func<TypeSet<T1, T2, T3, T4, T5, T6>, object>> exp);
        IExpSelect<T1, T2, T3, T4, T5, T6> Where(Expression<Func<TypeSet<T1, T2, T3, T4, T5, T6>, bool>> exp);
        IExpSelect<T1, T2, T3, T4, T5, T6> OrderBy(Expression<Func<TypeSet<T1, T2, T3, T4, T5, T6>, object>> exp, bool asc = true);
        IExpSelect<T1, T2, T3, T4, T5, T6> GroupBy(Expression<Func<TypeSet<T1, T2, T3, T4, T5, T6>, object>> exp);
    }

    public interface IExpSelect<T1, T2, T3, T4, T5, T6, T7> : IExpSelect0<IExpSelect<T1, T2, T3, T4, T5, T6, T7>, T1>
    {
        IEnumerable<TReturn> ToList<TReturn>(Expression<Func<TypeSet<T1, T2, T3, T4, T5, T6, T7>, object>> exp);
        Task<IList<TReturn>> ToListAsync<TReturn>(Expression<Func<TypeSet<T1, T2, T3, T4, T5, T6, T7>, object>> exp);
        IExpSelect<T1, T2, T3, T4, T5, T6, T7> Where(Expression<Func<TypeSet<T1, T2, T3, T4, T5, T6, T7>, bool>> exp);
        IExpSelect<T1, T2, T3, T4, T5, T6, T7> OrderBy(Expression<Func<TypeSet<T1, T2, T3, T4, T5, T6, T7>, object>> exp, bool asc = true);
        IExpSelect<T1, T2, T3, T4, T5, T6, T7> GroupBy(Expression<Func<TypeSet<T1, T2, T3, T4, T5, T6, T7>, object>> exp);
    }

    public interface IExpSelect<T1, T2, T3, T4, T5, T6, T7, T8> : IExpSelect0<IExpSelect<T1, T2, T3, T4, T5, T6, T7, T8>, T1>
    {
        IEnumerable<TReturn> ToList<TReturn>(Expression<Func<TypeSet<T1, T2, T3, T4, T5, T6, T7, T8>, object>> exp);
        Task<IList<TReturn>> ToListAsync<TReturn>(Expression<Func<TypeSet<T1, T2, T3, T4, T5, T6, T7, T8>, object>> exp);
        IExpSelect<T1, T2, T3, T4, T5, T6, T7, T8> Where(Expression<Func<TypeSet<T1, T2, T3, T4, T5, T6, T7, T8>, bool>> exp);
        IExpSelect<T1, T2, T3, T4, T5, T6, T7, T8> OrderBy(Expression<Func<TypeSet<T1, T2, T3, T4, T5, T6, T7, T8>, object>> exp, bool asc = true);
        IExpSelect<T1, T2, T3, T4, T5, T6, T7, T8> GroupBy(Expression<Func<TypeSet<T1, T2, T3, T4, T5, T6, T7, T8>, object>> exp);
    }

    public interface IExpSelect<T1, T2, T3, T4, T5, T6, T7, T8, T9> : IExpSelect0<IExpSelect<T1, T2, T3, T4, T5, T6, T7, T8, T9>, T1>
    {
        IEnumerable<TReturn> ToList<TReturn>(Expression<Func<TypeSet<T1, T2, T3, T4, T5, T6, T7, T8, T9>, object>> exp);
        Task<IList<TReturn>> ToListAsync<TReturn>(Expression<Func<TypeSet<T1, T2, T3, T4, T5, T6, T7, T8, T9>, object>> exp);
        IExpSelect<T1, T2, T3, T4, T5, T6, T7, T8, T9> Where(Expression<Func<TypeSet<T1, T2, T3, T4, T5, T6, T7, T8, T9>, bool>> exp);
        IExpSelect<T1, T2, T3, T4, T5, T6, T7, T8, T9> OrderBy(Expression<Func<TypeSet<T1, T2, T3, T4, T5, T6, T7, T8, T9>, object>> exp, bool asc = true);
        IExpSelect<T1, T2, T3, T4, T5, T6, T7, T8, T9> GroupBy(Expression<Func<TypeSet<T1, T2, T3, T4, T5, T6, T7, T8, T9>, object>> exp);
    }

    public interface IExpSelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> : IExpSelect0<IExpSelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>, T1>
    {
        IEnumerable<TReturn> ToList<TReturn>(Expression<Func<TypeSet<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>, object>> exp);
        Task<IList<TReturn>> ToListAsync<TReturn>(Expression<Func<TypeSet<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>, object>> exp);
        IExpSelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> Where(Expression<Func<TypeSet<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>, bool>> exp);
        IExpSelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> OrderBy(Expression<Func<TypeSet<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>, object>> exp, bool asc = true);
        IExpSelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> GroupBy(Expression<Func<TypeSet<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>, object>> exp);
    }

    public interface IExpSelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> : IExpSelect0<IExpSelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>, T1>
    {
        IEnumerable<TReturn> ToList<TReturn>(Expression<Func<TypeSet<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>, object>> exp);
        Task<IList<TReturn>> ToListAsync<TReturn>(Expression<Func<TypeSet<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>, object>> exp);
        IExpSelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> Where(Expression<Func<TypeSet<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>, bool>> exp);
        IExpSelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> OrderBy(Expression<Func<TypeSet<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>, object>> exp, bool asc = true);
        IExpSelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> GroupBy(Expression<Func<TypeSet<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>, object>> exp);
    }

    public interface IExpSelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> : IExpSelect0<IExpSelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>, T1>
    {
        IEnumerable<TReturn> ToList<TReturn>(Expression<Func<TypeSet<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>, object>> exp);
        Task<IList<TReturn>> ToListAsync<TReturn>(Expression<Func<TypeSet<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>, object>> exp);
        IExpSelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> Where(Expression<Func<TypeSet<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>, bool>> exp);
        IExpSelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> OrderBy(Expression<Func<TypeSet<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>, object>> exp, bool asc = true);
        IExpSelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> GroupBy(Expression<Func<TypeSet<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>, object>> exp);
    }
}

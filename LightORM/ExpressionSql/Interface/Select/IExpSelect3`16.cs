using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace MDbContext.ExpressionSql.Interface.Select;

public interface IExpSelect<T1, T2, T3> : IExpSelect0<IExpSelect<T1, T2, T3>, T1>
{
    //IEnumerable<TReturn> ToList<TReturn>(Expression<Func<TypeSet<T1, T2, T3>, object>> exp);
    //Task<IList<TReturn>> ToListAsync<TReturn>(Expression<Func<TypeSet<T1, T2, T3>, object>> exp);
    IExpSelect<T1, T2, T3> Where(Expression<Func<TypeSet<T1, T2, T3>, bool>> exp);
    IExpSelect<T1, T2, T3> OrderBy(Expression<Func<TypeSet<T1, T2, T3>, object>> exp, bool asc = true);
    IExpSelect<T1, T2, T3> GroupBy(Expression<Func<TypeSet<T1, T2, T3>, object>> exp);
    IExpSelect<T1, T2, T3> InnerJoin<TJoin>(Expression<Func<TypeSet<T1, T2, T3>, bool>> exp);
    IExpSelect<T1, T2, T3> LeftJoin<TJoin>(Expression<Func<TypeSet<T1, T2, T3>, bool>> exp);
    IExpSelect<T1, T2, T3> RightJoin<TJoin>(Expression<Func<TypeSet<T1, T2, T3>, bool>> exp);
}

public interface IExpSelect<T1, T2, T3, T4> : IExpSelect0<IExpSelect<T1, T2, T3, T4>, T1>
{
    //IEnumerable<TReturn> ToList<TReturn>(Expression<Func<TypeSet<T1, T2, T3, T4>, object>> exp);
    //Task<IList<TReturn>> ToListAsync<TReturn>(Expression<Func<TypeSet<T1, T2, T3, T4>, object>> exp);
    IExpSelect<T1, T2, T3, T4> Where(Expression<Func<TypeSet<T1, T2, T3, T4>, bool>> exp);
    IExpSelect<T1, T2, T3, T4> OrderBy(Expression<Func<TypeSet<T1, T2, T3, T4>, object>> exp, bool asc = true);
    IExpSelect<T1, T2, T3, T4> GroupBy(Expression<Func<TypeSet<T1, T2, T3, T4>, object>> exp);
    IExpSelect<T1, T2, T3, T4> InnerJoin<TJoin>(Expression<Func<TypeSet<T1, T2, T3, T4>, bool>> exp);
    IExpSelect<T1, T2, T3, T4> LeftJoin<TJoin>(Expression<Func<TypeSet<T1, T2, T3, T4>, bool>> exp);
    IExpSelect<T1, T2, T3, T4> RightJoin<TJoin>(Expression<Func<TypeSet<T1, T2, T3, T4>, bool>> exp);
}

public interface IExpSelect<T1, T2, T3, T4, T5> : IExpSelect0<IExpSelect<T1, T2, T3, T4, T5>, T1>
{
    //IEnumerable<TReturn> ToList<TReturn>(Expression<Func<TypeSet<T1, T2, T3, T4, T5>, object>> exp);
    //Task<IList<TReturn>> ToListAsync<TReturn>(Expression<Func<TypeSet<T1, T2, T3, T4, T5>, object>> exp);
    IExpSelect<T1, T2, T3, T4, T5> Where(Expression<Func<TypeSet<T1, T2, T3, T4, T5>, bool>> exp);
    IExpSelect<T1, T2, T3, T4, T5> OrderBy(Expression<Func<TypeSet<T1, T2, T3, T4, T5>, object>> exp, bool asc = true);
    IExpSelect<T1, T2, T3, T4, T5> GroupBy(Expression<Func<TypeSet<T1, T2, T3, T4, T5>, object>> exp);
    IExpSelect<T1, T2, T3, T4, T5> InnerJoin<TJoin>(Expression<Func<TypeSet<T1, T2, T3, T4, T5>, bool>> exp);
    IExpSelect<T1, T2, T3, T4, T5> LeftJoin<TJoin>(Expression<Func<TypeSet<T1, T2, T3, T4, T5>, bool>> exp);
    IExpSelect<T1, T2, T3, T4, T5> RightJoin<TJoin>(Expression<Func<TypeSet<T1, T2, T3, T4, T5>, bool>> exp);
}

public interface IExpSelect<T1, T2, T3, T4, T5, T6> : IExpSelect0<IExpSelect<T1, T2, T3, T4, T5, T6>, T1>
{
    //IEnumerable<TReturn> ToList<TReturn>(Expression<Func<TypeSet<T1, T2, T3, T4, T5, T6>, object>> exp);
    //Task<IList<TReturn>> ToListAsync<TReturn>(Expression<Func<TypeSet<T1, T2, T3, T4, T5, T6>, object>> exp);
    IExpSelect<T1, T2, T3, T4, T5, T6> Where(Expression<Func<TypeSet<T1, T2, T3, T4, T5, T6>, bool>> exp);
    IExpSelect<T1, T2, T3, T4, T5, T6> OrderBy(Expression<Func<TypeSet<T1, T2, T3, T4, T5, T6>, object>> exp, bool asc = true);
    IExpSelect<T1, T2, T3, T4, T5, T6> GroupBy(Expression<Func<TypeSet<T1, T2, T3, T4, T5, T6>, object>> exp);
    IExpSelect<T1, T2, T3, T4, T5, T6> InnerJoin<TJoin>(Expression<Func<TypeSet<T1, T2, T3, T4, T5, T6>, bool>> exp);
    IExpSelect<T1, T2, T3, T4, T5, T6> LeftJoin<TJoin>(Expression<Func<TypeSet<T1, T2, T3, T4, T5, T6>, bool>> exp);
    IExpSelect<T1, T2, T3, T4, T5, T6> RightJoin<TJoin>(Expression<Func<TypeSet<T1, T2, T3, T4, T5, T6>, bool>> exp);
}

public interface IExpSelect<T1, T2, T3, T4, T5, T6, T7> : IExpSelect0<IExpSelect<T1, T2, T3, T4, T5, T6, T7>, T1>
{
    //IEnumerable<TReturn> ToList<TReturn>(Expression<Func<TypeSet<T1, T2, T3, T4, T5, T6, T7>, object>> exp);
    //Task<IList<TReturn>> ToListAsync<TReturn>(Expression<Func<TypeSet<T1, T2, T3, T4, T5, T6, T7>, object>> exp);
    IExpSelect<T1, T2, T3, T4, T5, T6, T7> Where(Expression<Func<TypeSet<T1, T2, T3, T4, T5, T6, T7>, bool>> exp);
    IExpSelect<T1, T2, T3, T4, T5, T6, T7> OrderBy(Expression<Func<TypeSet<T1, T2, T3, T4, T5, T6, T7>, object>> exp, bool asc = true);
    IExpSelect<T1, T2, T3, T4, T5, T6, T7> GroupBy(Expression<Func<TypeSet<T1, T2, T3, T4, T5, T6, T7>, object>> exp);
    IExpSelect<T1, T2, T3, T4, T5, T6, T7> InnerJoin<TJoin>(Expression<Func<TypeSet<T1, T2, T3, T4, T5, T6, T7>, bool>> exp);
    IExpSelect<T1, T2, T3, T4, T5, T6, T7> LeftJoin<TJoin>(Expression<Func<TypeSet<T1, T2, T3, T4, T5, T6, T7>, bool>> exp);
    IExpSelect<T1, T2, T3, T4, T5, T6, T7> RightJoin<TJoin>(Expression<Func<TypeSet<T1, T2, T3, T4, T5, T6, T7>, bool>> exp);
}

public interface IExpSelect<T1, T2, T3, T4, T5, T6, T7, T8> : IExpSelect0<IExpSelect<T1, T2, T3, T4, T5, T6, T7, T8>, T1>
{
    //IEnumerable<TReturn> ToList<TReturn>(Expression<Func<TypeSet<T1, T2, T3, T4, T5, T6, T7, T8>, object>> exp);
    //Task<IList<TReturn>> ToListAsync<TReturn>(Expression<Func<TypeSet<T1, T2, T3, T4, T5, T6, T7, T8>, object>> exp);
    IExpSelect<T1, T2, T3, T4, T5, T6, T7, T8> Where(Expression<Func<TypeSet<T1, T2, T3, T4, T5, T6, T7, T8>, bool>> exp);
    IExpSelect<T1, T2, T3, T4, T5, T6, T7, T8> OrderBy(Expression<Func<TypeSet<T1, T2, T3, T4, T5, T6, T7, T8>, object>> exp, bool asc = true);
    IExpSelect<T1, T2, T3, T4, T5, T6, T7, T8> GroupBy(Expression<Func<TypeSet<T1, T2, T3, T4, T5, T6, T7, T8>, object>> exp);
    IExpSelect<T1, T2, T3, T4, T5, T6, T7, T8> InnerJoin<TJoin>(Expression<Func<TypeSet<T1, T2, T3, T4, T5, T6, T7, T8>, bool>> exp);
    IExpSelect<T1, T2, T3, T4, T5, T6, T7, T8> LeftJoin<TJoin>(Expression<Func<TypeSet<T1, T2, T3, T4, T5, T6, T7, T8>, bool>> exp);
    IExpSelect<T1, T2, T3, T4, T5, T6, T7, T8> RightJoin<TJoin>(Expression<Func<TypeSet<T1, T2, T3, T4, T5, T6, T7, T8>, bool>> exp);
}

public interface IExpSelect<T1, T2, T3, T4, T5, T6, T7, T8, T9> : IExpSelect0<IExpSelect<T1, T2, T3, T4, T5, T6, T7, T8, T9>, T1>
{
    //IEnumerable<TReturn> ToList<TReturn>(Expression<Func<TypeSet<T1, T2, T3, T4, T5, T6, T7, T8, T9>, object>> exp);
    //Task<IList<TReturn>> ToListAsync<TReturn>(Expression<Func<TypeSet<T1, T2, T3, T4, T5, T6, T7, T8, T9>, object>> exp);
    IExpSelect<T1, T2, T3, T4, T5, T6, T7, T8, T9> Where(Expression<Func<TypeSet<T1, T2, T3, T4, T5, T6, T7, T8, T9>, bool>> exp);
    IExpSelect<T1, T2, T3, T4, T5, T6, T7, T8, T9> OrderBy(Expression<Func<TypeSet<T1, T2, T3, T4, T5, T6, T7, T8, T9>, object>> exp, bool asc = true);
    IExpSelect<T1, T2, T3, T4, T5, T6, T7, T8, T9> GroupBy(Expression<Func<TypeSet<T1, T2, T3, T4, T5, T6, T7, T8, T9>, object>> exp);
    IExpSelect<T1, T2, T3, T4, T5, T6, T7, T8, T9> InnerJoin<TJoin>(Expression<Func<TypeSet<T1, T2, T3, T4, T5, T6, T7, T8, T9>, bool>> exp);
    IExpSelect<T1, T2, T3, T4, T5, T6, T7, T8, T9> LeftJoin<TJoin>(Expression<Func<TypeSet<T1, T2, T3, T4, T5, T6, T7, T8, T9>, bool>> exp);
    IExpSelect<T1, T2, T3, T4, T5, T6, T7, T8, T9> RightJoin<TJoin>(Expression<Func<TypeSet<T1, T2, T3, T4, T5, T6, T7, T8, T9>, bool>> exp);
}

public interface IExpSelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> : IExpSelect0<IExpSelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>, T1>
{
    //IEnumerable<TReturn> ToList<TReturn>(Expression<Func<TypeSet<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>, object>> exp);
    //Task<IList<TReturn>> ToListAsync<TReturn>(Expression<Func<TypeSet<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>, object>> exp);
    IExpSelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> Where(Expression<Func<TypeSet<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>, bool>> exp);
    IExpSelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> OrderBy(Expression<Func<TypeSet<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>, object>> exp, bool asc = true);
    IExpSelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> GroupBy(Expression<Func<TypeSet<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>, object>> exp);
    IExpSelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> InnerJoin<TJoin>(Expression<Func<TypeSet<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>, bool>> exp);
    IExpSelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> LeftJoin<TJoin>(Expression<Func<TypeSet<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>, bool>> exp);
    IExpSelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> RightJoin<TJoin>(Expression<Func<TypeSet<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>, bool>> exp);
}

public interface IExpSelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> : IExpSelect0<IExpSelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>, T1>
{
    //IEnumerable<TReturn> ToList<TReturn>(Expression<Func<TypeSet<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>, object>> exp);
    //Task<IList<TReturn>> ToListAsync<TReturn>(Expression<Func<TypeSet<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>, object>> exp);
    IExpSelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> Where(Expression<Func<TypeSet<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>, bool>> exp);
    IExpSelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> OrderBy(Expression<Func<TypeSet<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>, object>> exp, bool asc = true);
    IExpSelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> GroupBy(Expression<Func<TypeSet<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>, object>> exp);
    IExpSelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> InnerJoin<TJoin>(Expression<Func<TypeSet<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>, bool>> exp);
    IExpSelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> LeftJoin<TJoin>(Expression<Func<TypeSet<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>, bool>> exp);
    IExpSelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> RightJoin<TJoin>(Expression<Func<TypeSet<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>, bool>> exp);
}

public interface IExpSelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> : IExpSelect0<IExpSelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>, T1>
{
    //IEnumerable<TReturn> ToList<TReturn>(Expression<Func<TypeSet<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>, object>> exp);
    //Task<IList<TReturn>> ToListAsync<TReturn>(Expression<Func<TypeSet<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>, object>> exp);
    IExpSelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> Where(Expression<Func<TypeSet<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>, bool>> exp);
    IExpSelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> OrderBy(Expression<Func<TypeSet<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>, object>> exp, bool asc = true);
    IExpSelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> GroupBy(Expression<Func<TypeSet<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>, object>> exp);
    IExpSelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> InnerJoin<TJoin>(Expression<Func<TypeSet<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>, bool>> exp);
    IExpSelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> LeftJoin<TJoin>(Expression<Func<TypeSet<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>, bool>> exp);
    IExpSelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> RightJoin<TJoin>(Expression<Func<TypeSet<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>, bool>> exp);
}

public interface IExpSelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> : IExpSelect0<IExpSelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>, T1>
{
    //IEnumerable<TReturn> ToList<TReturn>(Expression<Func<TypeSet<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>, object>> exp);
    //Task<IList<TReturn>> ToListAsync<TReturn>(Expression<Func<TypeSet<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>, object>> exp);
    IExpSelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> Where(Expression<Func<TypeSet<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>, bool>> exp);
    IExpSelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> OrderBy(Expression<Func<TypeSet<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>, object>> exp, bool asc = true);
    IExpSelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> GroupBy(Expression<Func<TypeSet<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>, object>> exp);
    IExpSelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> InnerJoin<TJoin>(Expression<Func<TypeSet<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>, bool>> exp);
    IExpSelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> LeftJoin<TJoin>(Expression<Func<TypeSet<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>, bool>> exp);
    IExpSelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> RightJoin<TJoin>(Expression<Func<TypeSet<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>, bool>> exp);
}

public interface IExpSelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> : IExpSelect0<IExpSelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>, T1>
{
    //IEnumerable<TReturn> ToList<TReturn>(Expression<Func<TypeSet<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>, object>> exp);
    //Task<IList<TReturn>> ToListAsync<TReturn>(Expression<Func<TypeSet<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>, object>> exp);
    IExpSelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> Where(Expression<Func<TypeSet<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>, bool>> exp);
    IExpSelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> OrderBy(Expression<Func<TypeSet<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>, object>> exp, bool asc = true);
    IExpSelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> GroupBy(Expression<Func<TypeSet<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>, object>> exp);
    IExpSelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> InnerJoin<TJoin>(Expression<Func<TypeSet<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>, bool>> exp);
    IExpSelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> LeftJoin<TJoin>(Expression<Func<TypeSet<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>, bool>> exp);
    IExpSelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> RightJoin<TJoin>(Expression<Func<TypeSet<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>, bool>> exp);
}

public interface IExpSelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> : IExpSelect0<IExpSelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>, T1>
{
    //IEnumerable<TReturn> ToList<TReturn>(Expression<Func<TypeSet<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>, object>> exp);
    //Task<IList<TReturn>> ToListAsync<TReturn>(Expression<Func<TypeSet<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>, object>> exp);
    IExpSelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> Where(Expression<Func<TypeSet<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>, bool>> exp);
    IExpSelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> OrderBy(Expression<Func<TypeSet<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>, object>> exp, bool asc = true);
    IExpSelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> GroupBy(Expression<Func<TypeSet<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>, object>> exp);
    IExpSelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> InnerJoin<TJoin>(Expression<Func<TypeSet<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>, bool>> exp);
    IExpSelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> LeftJoin<TJoin>(Expression<Func<TypeSet<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>, bool>> exp);
    IExpSelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> RightJoin<TJoin>(Expression<Func<TypeSet<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>, bool>> exp);
}

public interface IExpSelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> : IExpSelect0<IExpSelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>, T1>
{
    //IEnumerable<TReturn> ToList<TReturn>(Expression<Func<TypeSet<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>, object>> exp);
    //Task<IList<TReturn>> ToListAsync<TReturn>(Expression<Func<TypeSet<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>, object>> exp);
    IExpSelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> Where(Expression<Func<TypeSet<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>, bool>> exp);
    IExpSelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> OrderBy(Expression<Func<TypeSet<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>, object>> exp, bool asc = true);
    IExpSelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> GroupBy(Expression<Func<TypeSet<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>, object>> exp);
    IExpSelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> InnerJoin<TJoin>(Expression<Func<TypeSet<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>, bool>> exp);
    IExpSelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> LeftJoin<TJoin>(Expression<Func<TypeSet<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>, bool>> exp);
    IExpSelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> RightJoin<TJoin>(Expression<Func<TypeSet<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>, bool>> exp);
}
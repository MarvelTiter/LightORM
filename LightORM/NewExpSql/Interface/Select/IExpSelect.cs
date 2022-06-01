﻿using MDbContext.NewExpSql.Providers.Select;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace MDbContext.NewExpSql.Interface.Select
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

        int Execute();
        string ToSql();
        TSelect Paging(int pageIndex, int pageSize);
        TMember Max<TMember>(Expression<Func<T1, TMember>> exp);
        double Sum(Expression<Func<T1, object>> exp);
        int Count(Expression<Func<T1, object>> exp);
    }
    public interface IExpSelect<T1> : IExpSelect0<IExpSelect<T1>, T1>
    {
        IEnumerable<TReturn> ToList<TReturn>(Expression<Func<T1, TReturn>> exp);
        IExpSelect<T1> Where(Expression<Func<T1, bool>> exp);
    }
    public interface IExpSelect<T1, T2> : IExpSelect0<IExpSelect<T1, T2>, T1>
    {
        IEnumerable<TReturn> ToList<TReturn>(Expression<Func<T1, T2, TReturn>> exp);
        IExpSelect<T1, T2> Where(Expression<Func<T1, T2, bool>> exp);
        IEnumerable<TReturn> ToList<TReturn>(Expression<Func<TypeSet<T1, T2>, TReturn>> exp);
        IExpSelect<T1, T2> Where(Expression<Func<TypeSet<T1, T2>, bool>> exp);
    }
    public interface IExpSelect<T1, T2, T3> : IExpSelect0<IExpSelect<T1, T2, T3>, T1>
    {
        IEnumerable<TReturn> ToList<TReturn>(Expression<Func<T1, T2, T3, TReturn>> exp);
        IExpSelect<T1, T2, T3> Where(Expression<Func<T1, T2, T3, bool>> exp);
        IEnumerable<TReturn> ToList<TReturn>(Expression<Func<TypeSet<T1, T2, T3>, TReturn>> exp);
        IExpSelect<T1, T2, T3> Where(Expression<Func<TypeSet<T1, T2, T3>, bool>> exp);
    }
    public interface IExpSelect<T1, T2, T3, T4> : IExpSelect0<IExpSelect<T1, T2, T3, T4>, T1>
    {
        IEnumerable<TReturn> ToList<TReturn>(Expression<Func<T1, T2, T3, T4, TReturn>> exp);
        IExpSelect<T1, T2, T3, T4> Where(Expression<Func<T1, T2, T3, T4, bool>> exp);
        IEnumerable<TReturn> ToList<TReturn>(Expression<Func<TypeSet<T1, T2, T3, T4>, TReturn>> exp);
        IExpSelect<T1, T2, T3, T4> Where(Expression<Func<TypeSet<T1, T2, T3, T4>, bool>> exp);
    }
    public interface IExpSelect<T1, T2, T3, T4, T5> { }
    public interface IExpSelect<T1, T2, T3, T4, T5, T6> { }
    public interface IExpSelect<T1, T2, T3, T4, T5, T6, T7> { }
    public interface IExpSelect<T1, T2, T3, T4, T5, T6, T7, T8> { }
    public interface IExpSelect<T1, T2, T3, T4, T5, T6, T7, T8, T9> { }
    public interface IExpSelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> { }
    public interface IExpSelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> { }

    public static class SelectExtension
    {
        public static IExpSelect<T1, T2> Select<T1, T2>(this IExpSql self, string key = "MainDb") where T1 : class, new()
        {
            var ins = self as ExpressionSql;
            return new SelectProvider2<T1, T2>(key, ins.GetContext, ins.GetDbInfo(key));
        }
        public static IExpSelect<T1, T2, T3> Select<T1, T2, T3>(this IExpSql self, string key = "MainDb") where T1 : class, new()
        {
            var ins = self as ExpressionSql;
            return new SelectProvider3<T1, T2, T3>(key, ins.GetContext, ins.GetDbInfo(key));
        }
    }
}

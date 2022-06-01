using MDbContext.NewExpSql.Ado;
using MDbContext.NewExpSql.Interface.Select;
using MDbContext.NewExpSql.Providers;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace MDbContext.NewExpSql.Interface
{
    public interface IExpSql
    {
        IExpSelect<T> Select<T>(string key = "MainDb");
        IExpInsert<T> Insert<T>(string key = "MainDb");
        IExpUpdate<T> Update<T>(string key = "MainDb");
        IExpDelete<T> Delete<T>(string key = "MainDb");
        IAdo Ado { get; }
    }

    public static class ExpSqlExtensions
    {
        //public static IExpSelect<T> Select<T,T1>(this IExpSql self)
        //{
        //    return new SelectProvider<T>()
        //}
    }
}

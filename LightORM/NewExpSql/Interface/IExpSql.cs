using System;
using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;
using System.Text;

namespace MDbContext.NewExpSql.Interface
{
    public interface IExpSql
    {
        IExpSelect<T> Select<T>();
        IExpInsert<T> Insert<T>();
        IExpUpdate<T> Update<T>();
        IExpDelete<T> Delete<T>();
    }
}

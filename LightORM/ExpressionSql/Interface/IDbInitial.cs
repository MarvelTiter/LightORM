using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MDbContext.ExpressionSql.Interface
{
    public interface IDbInitial
    {
        IDbInitial CreateTable<T>(string key= ConstString.Main);
    }
}

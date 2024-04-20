﻿using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace LightORM;

public interface IExpInsert<T>
{
    //IExpInsert<T> AppendData(T item);
    //IExpInsert<T> AppendData(IEnumerable<T> items);
    IExpInsert<T> SetColumns(Expression<Func<T, object>> columns);
    IExpInsert<T> IgnoreColumns(Expression<Func<T, object>> columns);
    int Execute();
    //IExpInsert<T> NoParameter();
    Task<int> ExecuteAsync();
    string ToSql();
}

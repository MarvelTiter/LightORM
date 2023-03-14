using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace MDbContext.ExpressionSql.Interface
{
    public interface IExpInsert<T>: ITransactionable
    {
        IExpInsert<T> AppendData(T item);
        IExpInsert<T> AppendData(IEnumerable<T> items);
        IExpInsert<T> SetColumns(Expression<Func<T, object>> columns);
        IExpInsert<T> IgnoreColumns(Expression<Func<T, object>> columns);
        int Execute();
#if NET40
#else
        Task<int> ExecuteAsync();
		IExpInsert<T> AttachCancellationToken(CancellationToken token);
#endif
		string ToSql();
    }
}

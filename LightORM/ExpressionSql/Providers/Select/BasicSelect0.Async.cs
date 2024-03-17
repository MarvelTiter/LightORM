#if NET40
#else
using LightORM.ExpressionSql.ExpressionVisitor;
using LightORM.ExpressionSql.Interface.Select;
using LightORM.Interfaces;
using LightORM.SqlExecutor;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LightORM.ExpressionSql.Providers.Select
{
    internal partial class BasicSelect0<TSelect, T1> : BasicProvider<T1>, IExpSelect0<TSelect, T1> where TSelect : class, IExpSelect0
	{
		public TSelect AttachCancellationToken(CancellationToken token)
		{
			CancellToken = token;
			return (this as TSelect)!;
		}
		public Task<TMember> MaxAsync<TMember>(Expression<Func<T1, TMember>> exp)
		{
			select ??= new SqlFragment();
			context.SetFragment(select);
			select.Append("MAX(");
			ExpressionVisit.Visit(exp.Body, SqlResolveOptions.SelectFunc, context);
			// tosql去掉最后2个字符
			select.Append(")))");
			SqlArgs args = BuildArgs();
			return InternalSingleAsync<TMember>(args);
		}

		public Task<double> SumAsync(Expression<Func<T1, object>> exp)
		{
			select ??= new SqlFragment();
			context.SetFragment(select);
			select.Append("SUM(");
			ExpressionVisit.Visit(exp.Body, SqlResolveOptions.SelectFunc, context);
			// tosql去掉最后2个字符
			select.Append(")))");
			SqlArgs args = BuildArgs();
			return InternalSingleAsync<double>(args);
		}

		public Task<int> CountAsync(Expression<Func<T1, object>> exp)
		{
			select ??= new SqlFragment();
			context.SetFragment(select);
			select.Append("COUNT(");
			ExpressionVisit.Visit(exp.Body, SqlResolveOptions.SelectFunc, context);
			// tosql去掉最后2个字符
			select.Append(")))");
			SqlArgs args = BuildArgs();
			return InternalSingleAsync<int>(args);
		}

		public Task<int> CountAsync()
		{
			select ??= new SqlFragment();
			context.SetFragment(select);
			select.Append("COUNT(*");
			// tosql去掉最后2个字符
			select.Append(")))");
			SqlArgs args = BuildArgs();
			return InternalSingleAsync<int>(args);
		}

		public async Task<bool> AnyAsync()
		{
			return await CountAsync() > 0;
		}

		public Task<IList<T1>> ToListAsync(Expression<Func<T1, object>> exp)
		{
			SelectHandle(exp.Body);
			var args = BuildArgs();
			return InternalQueryAsync<T1>(args);
		}
		public Task<IList<T1>> ToListAsync()
		{
			SelectHandle(selectBody);
			var args = BuildArgs();
			return InternalQueryAsync<T1>(args);
		}
		public Task<IList<dynamic>> ToDynamicListAsync()
		{
			SelectHandle(selectBody);
			var args = BuildArgs();
			return InternalQueryAsync(args);
		}

		public Task<IList<TReturn>> ToListAsync<TReturn>()
		{
			//Expression<Func<TReturn, object>> exp = r => new { r };
			//SelectHandle(exp.Body);
			SelectHandle(selectBody);
			var args = BuildArgs();
			return InternalQueryAsync<TReturn>(args);
		}
		public Task<DataTable> ToDataTableAsync()
		{
			//using var conn = dbConnect.CreateConnection();
			//var args = BuildArgs();
			//Life.BeforeExecute?.Invoke(args);
			//var ret = conn.ExecuteTableAsync(args.Sql!, args.SqlParameter);
			//args.Done = true;
			//Life.AfterExecute?.Invoke(args);
			//return ret;
			throw new NotImplementedException();
		}
	}
}
#endif

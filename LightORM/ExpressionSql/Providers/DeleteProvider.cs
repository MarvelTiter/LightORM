using MDbContext;
using MDbContext.ExpressionSql.Interface;
using MDbContext.SqlExecutor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace MDbContext.ExpressionSql.Providers;

internal partial class DeleteProvider<T> : BasicProvider<T>, IExpDelete<T>
{
    public DeleteProvider(string key, Func<string, ITableContext> getContext, DbConnectInfo connectInfos, SqlExecuteLife life)
  : base(key, getContext, connectInfos, life) { }

    protected override SqlConfig WhereConfig => SqlConfig.DeleteWhere;

    public int Execute()
    {
        using var conn = dbConnect.CreateConnection();
        var param = context.GetParameters();
        return conn.Execute(ToSql(), param);
    }

    public async Task<int> ExecuteAsync()
    {
        using var conn = dbConnect.CreateConnection();
        var param = context.GetParameters();
        return await conn.ExecuteAsync(ToSql(), param);
    }

    public string ToSql()
    {
        StringBuilder sql = new StringBuilder();
        var table = context.Tables.Values.First();
        sql.Append($"DELETE FROM {table.TableName} ");
        sql.Append($"WHERE {where}");
        Life.BeforeExecute?.Invoke(sql.ToString());
        return sql.ToString();
    }

    public IExpDelete<T> Where(T item)
    {
        Expression<Func<object>> exp = () => item;
        WhereHandle(exp.Body);
        return this;
    }

    public IExpDelete<T> Where(IEnumerable<T> items)
    {
        throw new NotImplementedException();
    }

    public IExpDelete<T> Where(Expression<Func<T, bool>> exp)
    {
        WhereHandle(exp.Body);
        return this;
    }

    public IExpDelete<T> WhereIf(bool condition, Expression<Func<T, bool>> exp)
    {
        if (condition) Where(exp);
        return this;
    }
}

using MDbContext;
using MDbContext.ExpressionSql.ExpressionVisitor;
using MDbContext.ExpressionSql.Interface;
using MDbContext.SqlExecutor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace MDbContext.ExpressionSql.Providers;

internal partial class UpdateProvider<T> : BasicProvider<T>, IExpUpdate<T>
{
    SqlFragment? ignore;
    SqlFragment? update;
    public UpdateProvider(string key, Func<string, ITableContext> getContext, DbConnectInfo connectInfos, SqlExecuteLife life)
  : base(key, getContext, connectInfos, life) { }

    protected override SqlConfig WhereConfig => SqlConfig.UpdateWhere;

    public IExpUpdate<T> AppendData(T item)
    {
        update ??= new SqlFragment();
        Expression<Func<object>> exp = () => item;
        context.SetFragment(update);
        ExpressionVisit.Visit(exp.Body, SqlConfig.Update, context);
        return this;
    }

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

    public IExpUpdate<T> IgnoreColumns(Expression<Func<T, object>> columns)
    {
        ignore ??= new SqlFragment();
        context.SetFragment(ignore);
        ExpressionVisit.Visit(columns.Body, SqlConfig.UpdateIgnore, context);
        return this;
    }
    public IExpUpdate<T> Set<TField>(Expression<Func<T, TField>> exp)
    {
        update ??= new SqlFragment();
        context.SetFragment(update);
        ExpressionVisit.Visit(exp.Body, SqlConfig.Update, context);
        return this;
    }

    public IExpUpdate<T> SetIf<TField>(bool condition, Expression<Func<T, TField>> exp)
    {
        if (condition) return Set(exp);
        return this;
    }

    public string ToSql()
    {
        StringBuilder sql = new StringBuilder();
        var table = context.Tables.Values.First();
        sql.Append($"UPDATE {table.TableName} SET");
        for (int i = 0; i < update.Names.Count; i++)
        {
            var f = update.Names[i];
            if (ignore?.Has(f) ?? false)
                continue;
            sql.Append($"\n{f} = {update.Values[i]},");
        }
        sql.Remove(sql.Length - 1, 1);
        sql.Append($"\nWHERE {where}");
        Life.BeforeExecute?.Invoke(sql.ToString());
        return sql.ToString();
    }

    public IExpUpdate<T> UpdateColumns(Expression<Func<object>> columns)
    {
        update ??= new SqlFragment();
        context.SetFragment(update);
        ExpressionVisit.Visit(columns.Body, SqlConfig.Update, context);
        return this;
    }

    public IExpUpdate<T> Where(T item)
    {
        Expression<Func<object>> exp = () => item;
        WhereHandle(exp.Body);
        return this;
    }

    public IExpUpdate<T> Where(IEnumerable<T> items)
    {
        throw new NotImplementedException();
    }

    public IExpUpdate<T> Where(Expression<Func<T, bool>> exp)
    {
        WhereHandle(exp.Body);
        return this;
    }

    public IExpUpdate<T> WhereIf(bool condition, Expression<Func<T, bool>> exp)
    {
        if (condition) Where(exp);
        return this;
    }
}

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
    public UpdateProvider(string key, ITableContext getContext, DbConnectInfo connectInfos, SqlExecuteLife life)
  : base(getContext, connectInfos, life)
    {
        DbKey = key;
    }

    protected override SqlConfig WhereConfig => SqlConfig.UpdateWhere;

    public void AttachTransaction()
    {
        Life.Core!.Attch(ToSql(), context.GetParameters(), DbKey!);
    }

    public IExpUpdate<T> AppendData(T item)
    {
        update ??= new SqlFragment();
        Expression<Func<object>> exp = () => item!;
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
    public IExpUpdate<T> Set<TField>(Expression<Func<T, TField>> exp, object value)
    {
        update ??= new SqlFragment();
        context.SetFragment(update);
        ExpressionVisit.Visit(exp.Body, SqlConfig.UpdatePartial, context);
        context.AppendDbParameter(value);
        return this;
    }

    public IExpUpdate<T> SetIf<TField>(bool condition, Expression<Func<T, TField>> exp, object value)
    {
        if (condition) return Set(exp, value);
        return this;
    }

    public string ToSql()
    {
        StringBuilder sql = new StringBuilder();
        var table = context.Tables.First();
        var primary = table.Fields!.Values.Where(f => f.IsPrimaryKey);
        bool updateKey = false;
        if (where == null)
        {
            if (!primary.Any()) throw new InvalidOperationException($"Where Condition is null and Model of [{table.CsName}] do not has a PrimaryKey");
            updateKey = true;
            where = new SqlFragment();
        }
        sql.Append($"UPDATE {table.TableName} SET");
        for (int i = 0; i < update!.Names.Count; i++)
        {
            var f = update.Names[i];
            if (primary.Any(sqlf => sqlf.FieldName == f))
            {
                if (updateKey)
                {
                    if (where.Length > 0) where.Append("AND ");
                    where.Append($"{context.DbHandler.ColumnEmphasis(f)} = {update.Values[i]}");
                }
                continue;
            }
            if (ignore?.Has(f) ?? false)
                continue;
            sql.Append($"\n{context.DbHandler.ColumnEmphasis(f)} = {update.Values[i]},");
        }
        if (update!.Names.Count > 0)
            sql.Remove(sql.Length - 1, 1);
        if (where!.Length == 0) throw new InvalidOperationException($"Where Condition is null");
        sql.Append($"\nWHERE {where}");
        Life.BeforeExecute?.Invoke(new SqlArgs { Sql = sql.ToString() });
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
        Expression<Func<object>> exp = () => item!;
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

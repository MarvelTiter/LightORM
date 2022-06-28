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

internal partial class InsertProvider<T> : BasicProvider<T>, IExpInsert<T>
{
    SqlFragment? ignore;
    SqlFragment? insert;
    public InsertProvider(string key, Func<string, ITableContext> getContext, DbConnectInfo connectInfos, SqlExecuteLife life)
  : base(key, getContext, connectInfos,life) { }

    protected override SqlConfig WhereConfig => throw new NotImplementedException();

    public IExpInsert<T> AppendData(T item)
    {
        insert ??= new SqlFragment();
        Expression<Func<object>> exp = () => item;
        context.SetFragment(insert);
        ExpressionVisit.Visit(exp.Body, SqlConfig.Insert, context);
        return this;
    }

    public IExpInsert<T> AppendData(IEnumerable<T> items)
    {
        throw new NotImplementedException();
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

    public IExpInsert<T> IgnoreColumns(Expression<Func<T, object>> columns)
    {
        ignore ??= new SqlFragment();
        context.SetFragment(ignore);
        ExpressionVisit.Visit(columns.Body, SqlConfig.InsertIgnore, context);
        return this;
    }

    public IExpInsert<T> SetColumns(Expression<Func<T, object>> columns)
    {
        insert ??= new SqlFragment();
        context.SetFragment(insert);
        ExpressionVisit.Visit(columns.Body, SqlConfig.Insert, context);
        return this;
    }

    public string ToSql()
    {
        StringBuilder sql = new StringBuilder();
        var table = context.Tables.Values.First();
        sql.Append($"INSERT INTO {table.TableName} () VALUES ()");
        var fIndex = 11 + table.TableName.Length + 3;
        var vIndex = fIndex + 10;
        for (int i = 0; i < insert.Names.Count; i++)
        {
            var f = insert.Names[i];
            if (ignore?.Has(f) ?? false)
                continue;
            sql.Insert(fIndex, $"{f},");
            // 逗号 Length + 1;
            fIndex += f.Length + 1;
            vIndex += f.Length + 1;
            var p = insert.Values[i];
            sql.Insert(vIndex, $"{p},");
            vIndex += p.Length + 1;
        }
        if (insert.Names.Count > 0)
        {
            // 移除最后面的逗号
            sql.Remove(fIndex - 1, 1);
            sql.Remove(vIndex - 2, 1);
        }
        Life.BeforeExecute?.Invoke(sql.ToString());
        return sql.ToString();
    }

    //IExpInsert<T> ISql<IExpInsert<T>, T>.Where(Expression<Func<T, bool>> exp)
    //{
    //    throw new NotImplementedException();
    //}

    //IExpInsert<T> ISql<IExpInsert<T>, T>.WhereIf(bool condition, Expression<Func<T, bool>> exp)
    //{
    //    throw new NotImplementedException();
    //}
}

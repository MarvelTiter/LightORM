using LightORM.ExpressionSql.ExpressionVisitor;
using LightORM.ExpressionSql.Interface;

/* 项目“LightORM (net4.0)”的未合并的更改
在此之前:
using MDbContext;
在此之后:
using LightORM.ExpressionSql.Providers;
using MDbContext;
*/
using LightORM.ExpressionSql.Providers;
using LightORM.SqlExecutor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LightORM.ExpressionSql.Providers;

internal partial class DeleteProvider<T> : BasicProvider<T>, IExpDelete<T>
{
    public DeleteProvider(string key, ITableContext getContext, DbConnectInfo connectInfos, SqlExecuteLife life)
  : base(getContext, connectInfos, life)
    {
        DbKey = key;
    }

    protected override SqlConfig WhereConfig => SqlConfig.DeleteWhere;

    public void AttachTransaction()
    {
        Life.Core!.Attch(ToSql(), context.GetParameters(), DbKey!);
    }

    public int Execute()
    {
        //using var conn = dbConnect.CreateConnection();
        //var param = context.GetParameters();
        //return conn.Execute(ToSql(), param);
        SqlArgs args = new SqlArgs
        {
            Sql = ToSql(),
            SqlParameter = context.GetParameters(),
            Action = SqlAction.Delete,
        };
        return InternalExecute(args);
    }
#if NET40
#else
    public Task<int> ExecuteAsync()
    {
        //using var conn = dbConnect.CreateConnection();
        //var param = context.GetParameters();
        //return await conn.ExecuteAsync(ToSql(), param);
        SqlArgs args = new SqlArgs
        {
            Sql = ToSql(),
            SqlParameter = context.GetParameters(),
            Action = SqlAction.Delete,
        };
        return InternalExecuteAsync(args);
    }

    public IExpDelete<T> AttachCancellationToken(CancellationToken token)
    {
        CancellToken = token;
        return this;
    }

#endif
    public string ToSql()
    {
        StringBuilder sql = new StringBuilder();
        var table = context.Tables.First();
        var primary = table.Fields!.Values.Where(f => f.IsPrimaryKey);
        bool deleteKey = false;
        if (where == null)
        {
            deleteKey = true;
            where = new SqlFragment();
        }
        sql.Append($"DELETE FROM {context.DbHandler.DbEmphasis(table.TableName!)} ");
        if (deleteKey)
        {
            if (!primary.Any()) throw new InvalidOperationException($"Where Condition is null and Model of [{table.CsName}] do not has a PrimaryKey");
            foreach (var p in primary)
            {
                var i = delete?.Names.IndexOf(p.FieldName!) ?? -1;
                if (i < 0) continue;
                if (where.Length > 0) where.Append("AND ");
                where.Append($"{context.DbHandler.DbEmphasis(delete!.Names[i])} = {delete?.Values[i]}");
            }
        }
        sql.Append($"WHERE {where}");
        //Life.BeforeExecute?.Invoke(new SqlArgs { Sql = sql.ToString(), SqlParameter = context.GetParameters(), Action = SqlAction.Delete });
        return sql.ToString();
    }
    SqlFragment? delete;
    public IExpDelete<T> AppendData(T item)
    {
        Expression<Func<object>> exp = () => item!;
        delete ??= new SqlFragment();
        context.SetFragment(delete);
        ExpressionVisit.Visit(exp.Body, SqlConfig.Delete, context);
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

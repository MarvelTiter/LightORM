using LightORM.Abstracts.Builder;
using LightORM.ExpressionSql.ExpressionVisitor;
using LightORM.Interfaces;
using LightORM.SqlExecutor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace LightORM.ExpressionSql.Providers;

internal abstract partial class BasicProvider<T1>
{
    protected readonly SqlContext context;
    protected readonly DbConnectInfo dbConnect;
    protected readonly Dictionary<string, SqlFieldInfo> SessionFields = new Dictionary<string, SqlFieldInfo>();
    protected SqlFragment? where;

    public string? DbKey { get; set; }
    public SqlExecuteLife Life { get; }

    public BasicProvider(ITableContext tableContext, DbConnectInfo connectInfos, SqlExecuteLife life)
    {
        //var tbContext = getContext.Invoke(key);

        //this.tableContext = tableContext;
        dbConnect = connectInfos;
        Life = life;
        context = new SqlContext(tableContext);
        //tables = new List<TableInfo>();
        context.AddTable(typeof(T1));
    }

    protected abstract SqlResolveOptions WhereConfig { get; }
    protected virtual void WhereHandle(Expression body)
    {
        where ??= new SqlFragment();
        if (where.Length > 0) where.Append("\nAND ");
        where.Append("(");
        if (body?.ToString() == "True")
        {
            where.Append(" 1 = 1 ");
        }
        else
        {
            context.SetFragment(where);
            ExpressionVisit.Visit(body, WhereConfig, context);
        }
        where.Append(")");
    }

    internal TReturn InternalSingle<TReturn>(SqlArgs args)
    {
        //Life.BeforeExecute?.Invoke(args);
        //using var conn = dbConnect.CreateConnection();
        //var ret = conn.QuerySingle<TReturn>(args.Sql!, args.SqlParameter);
        //args.Done = true;
        //Life.AfterExecute?.Invoke(args);
        //return ret;
        throw new NotImplementedException();
    }


    internal int InternalExecute(SqlArgs args)
    {
        //Life.BeforeExecute?.Invoke(args);
        //using var conn = dbConnect.CreateConnection();
        //var ret = conn.Execute(args.Sql!, args.SqlParameter);
        //args.Done = true;
        //Life.AfterExecute?.Invoke(args);
        //return ret;
        throw new NotImplementedException();
    }



    //internal TReturn InternalExecute<TReturn>(string sql, object param)
    //{
    //    using var conn = dbConnect.CreateConnection();
    //    return conn.QuerySingle<TReturn>(sql, param);
    //}

    internal  IEnumerable<TReturn> InternalQuery<TReturn>(SqlArgs args)
    {
        //Life.BeforeExecute?.Invoke(args);
        //var conn = dbConnect.CreateConnection();
        //var ret = conn.Query<TReturn>(args.Sql!, args.SqlParameter);
        //args.Done = true;
        //Life.AfterExecute?.Invoke(args);
        //return ret;
        throw new NotImplementedException();
    }

    internal IEnumerable<dynamic> InternalQuery(SqlArgs args)
    {
        //var sql = ToSql();
        //var param = context.GetParameters();
        //Life.BeforeExecute?.Invoke(args);
        //var conn = dbConnect.CreateConnection();
        //var ret = conn.Query(args.Sql!, args.SqlParameter);
        //args.Done = true;
        //Life.AfterExecute?.Invoke(args);
        //return ret;
        throw new NotImplementedException();
    }

    internal object InternalScale(SqlArgs args)
    {
        //Life.BeforeExecute?.Invoke(args);
        //var conn = dbConnect.CreateConnection();
        //var ret = conn.ExecuteScale(args.Sql!, args.SqlParameter);
        //args.Done = true;
        //Life.AfterExecute?.Invoke(args);
        //return ret;
        throw new NotImplementedException();
    }   
}

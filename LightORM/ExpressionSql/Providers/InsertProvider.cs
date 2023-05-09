using MDbContext;
using MDbContext.ExpressionSql.ExpressionVisitor;
using MDbContext.ExpressionSql.Interface;
using MDbContext.SqlExecutor;
using MDbContext.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MDbContext.ExpressionSql.Providers;

internal partial class InsertProvider<T> : BasicProvider<T>, IExpInsert<T>
{
    SqlFragment? ignore;
    SqlFragment? insert;
    public InsertProvider(string key, ITableContext getContext, DbConnectInfo connectInfos, SqlExecuteLife life)
  : base(getContext, connectInfos, life)
    {
        DbKey = key;
    }

    protected override SqlConfig WhereConfig => throw new NotImplementedException();

    public void AttachTransaction()
    {
        Life.Core!.Attch(ToSql(), context.GetParameters(), DbKey!);
    }
    IEnumerable<SimpleColumn>? entityColumns;
    List<T>? entities;
    public IExpInsert<T> AppendData(T item)
    {
        insert ??= new SqlFragment();
        context.SetFragment(insert);
        //Expression<Func<object>> exp = () => item!;
        //ExpressionVisit.Visit(exp.Body, SqlConfig.Insert, context);
        entityColumns ??= typeof(T).GetColumns();
        entities ??= new List<T>();
        entities.Add(item);
        return this;
    }

    public IExpInsert<T> AppendData(IEnumerable<T> items)
    {
        insert ??= new SqlFragment();
        context.SetFragment(insert);
        //Expression<Func<object>> exp = () => item!;
        //ExpressionVisit.Visit(exp.Body, SqlConfig.Insert, context);
        entityColumns ??= typeof(T).GetColumns();
        entities ??= new List<T>();
        entities.AddRange(items);
        return this;
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
            Action = SqlAction.Insert,
        };
        return InternalExecute(args);
    }
#if !NET40
    public Task<int> ExecuteAsync()
    {
        SqlArgs args = new SqlArgs
        {
            Sql = ToSql(),
            SqlParameter = context.GetParameters(),
            Action = SqlAction.Insert,
        };
        return InternalExecuteAsync(args);
        //using var conn = dbConnect.CreateConnection();
        //var param = context.GetParameters();
        //return await conn.ExecuteAsync(ToSql(), param);
    }

	public IExpInsert<T> AttachCancellationToken(CancellationToken token)
	{
		CancellToken = token;
		return this;
	}

#endif

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
        if (entities != null)
        {
            return BuildEntitySql();
        }
        else
        {
            return BuildFragmentSql();
        }

        string BuildFragmentSql()
        {
            StringBuilder sql = new StringBuilder();
            var table = context.Tables.First();
            sql.Append($"INSERT INTO {table.TableName} () VALUES ()");
            var fIndex = 11 + table.TableName!.Length + 3;
            var vIndex = fIndex + 10;
            for (int i = 0; i < insert!.Names.Count; i++)
            {
                var f = insert.Names[i];
                if (ignore?.Has(f) ?? false)
                    continue;
                sql.Insert(fIndex, $"{context.DbHandler.ColumnEmphasis(f)},");
                // 逗号、中括号 Length + 3;
                fIndex += f.Length + 3;
                vIndex += f.Length + 3;
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
            //Life.BeforeExecute?.Invoke(new SqlArgs { Sql = sql.ToString(), SqlParameter = context.GetParameters(), Action = SqlAction.Insert });
            return sql.ToString();
        }

        string BuildEntitySql()
        {
            StringBuilder sql = new StringBuilder();
            var table = context.Tables.First();
            sql.Append($"INSERT INTO {table.TableName} (");
            var insertColumnIndex = sql.Length;
            sql.Append(")\n");
            sql.AppendLine("VALUES");
            foreach (var item in entities)
            {
                sql.Append("(");
                List<SimpleColumn> insertCols = new List<SimpleColumn>();
                foreach (var col in entityColumns!)
                {
                    var val = item.AccessValue(col.PropName!);
                    if (val == null)
                    {
                        col.NullValue = true;
                    }
                    else
                    {
                        var pName = context.AppendDbParameter(val);
                        sql.Append(pName);
                        sql.Append(", ");
                    }
                    insertCols.Add(col);
                }
                sql.Insert(insertColumnIndex, string.Join(", ", insertCols.Where(c => c.Insertable).Select(c => context.DbHandler.ColumnEmphasis(c.DbColumn!))));
                sql.Remove(sql.Length - 2, 2);
                sql.Append("),\n");
            }
            sql.Remove(sql.Length - 2, 2);
            return sql.ToString();
        }
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

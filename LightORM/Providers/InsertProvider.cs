using LightORM.Builder;
using System.Threading.Tasks;

namespace LightORM.Providers;

internal sealed class InsertProvider<T> : IExpInsert<T>
{
    private readonly ISqlExecutor executor;
    InsertBuilder SqlBuilder = new InsertBuilder();
    public InsertProvider(ISqlExecutor executor, T? entity)
    {
        this.executor = executor;
        SqlBuilder.DbType = this.executor.ConnectInfo.DbBaseType;
        SqlBuilder.TableInfo = Cache.TableContext.GetTableInfo<T>();
        SqlBuilder.TargetObject = entity;
    }

    public InsertProvider(ISqlExecutor executor, IEnumerable<T> entities)
    {
        this.executor = executor;
        SqlBuilder.DbType = this.executor.ConnectInfo.DbBaseType;
        SqlBuilder.TableInfo = Cache.TableContext.GetTableInfo<T>();
        SqlBuilder.TargetObject = entities;
        SqlBuilder.IsInsertList = true;
    }
   
    public int Execute()
    {
        var sql = SqlBuilder.ToSqlString();
        var parameters = SqlBuilder.DbParameters;
        return executor.ExecuteNonQuery(sql, parameters);
    }

    public Task<int> ExecuteAsync()
    {
        var sql = SqlBuilder.ToSqlString();
        var parameters = SqlBuilder.DbParameters;
        return executor.ExecuteNonQueryAsync(sql, parameters);
    }

    public IExpInsert<T> IgnoreColumns(Expression<Func<T, object>> columns)
    {
        SqlBuilder.Expressions.Add(new ExpressionInfo()
        {
            Expression = columns,
            ResolveOptions = SqlResolveOptions.InsertIgnore,
        });
        return this;
    }

    public IExpInsert<T> SetColumns(Expression<Func<T, object>> columns)
    {
        SqlBuilder.Expressions.Add(new ExpressionInfo()
        {
            Expression = columns,
            ResolveOptions = SqlResolveOptions.Insert,
        });
        return this;
    }

    public string ToSql() => SqlBuilder.ToSqlString();
}

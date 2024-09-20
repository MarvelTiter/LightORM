﻿using LightORM.Builder;
using LightORM.Extension;
using System.Threading.Tasks;

namespace LightORM.Providers;

internal sealed class InsertProvider<T> : IExpInsert<T>
{
    private readonly ISqlExecutor executor;
    InsertBuilder<T> SqlBuilder = new InsertBuilder<T>();
    public InsertProvider(ISqlExecutor executor, T? entity)
    {
        this.executor = executor;
        SqlBuilder.DbType = this.executor.ConnectInfo.DbBaseType;
        SqlBuilder.MainTable = Cache.TableContext.GetTableInfo<T>();
        SqlBuilder.TargetObject = entity;
    }

    public InsertProvider(ISqlExecutor executor, IEnumerable<T> entities)
    {
        this.executor = executor;
        SqlBuilder.DbType = this.executor.ConnectInfo.DbBaseType;
        SqlBuilder.MainTable = Cache.TableContext.GetTableInfo<T>();
        SqlBuilder.TargetObjects = entities;
        SqlBuilder.IsBatchInsert = true;
    }

    public int Execute()
    {
        var sql = SqlBuilder.ToSqlString();
        if (SqlBuilder.IsBatchInsert)
        {
            var isTran = executor.DbTransaction == null;
            try
            {
                var effectRows = 0;
                if (!isTran)
                {
                    executor.BeginTran();
                    isTran = true;
                }
                foreach (var item in SqlBuilder.BatchInfos!)
                {
                    effectRows += executor.ExecuteNonQuery(item.Sql!, item.ToDictionaryParameters());
                }
                if (isTran)
                {
                    executor.CommitTran();
                }
                return effectRows;
            }
            catch
            {
                if (isTran)
                {
                    executor.RollbackTran();
                }
                throw;
            }

        }
        else
        {
            var dbParameters = SqlBuilder.DbParameters;
            return executor.ExecuteNonQuery(sql, dbParameters);
        }
    }

    public async Task<int> ExecuteAsync()
    {
        var sql = SqlBuilder.ToSqlString();
        if (SqlBuilder.IsBatchInsert)
        {
            var isTran = executor.DbTransaction == null;
            try
            {
                var effectRows = 0;
                if (!isTran)
                {
                    await executor.BeginTranAsync();
                    isTran = true;
                }
                foreach (var item in SqlBuilder.BatchInfos!)
                {
                    effectRows += await executor.ExecuteNonQueryAsync(item.Sql!, item.ToDictionaryParameters());
                }
                if (isTran)
                {
                    await executor.CommitTranAsync();
                }
                return effectRows;
            }
            catch
            {
                if (isTran)
                {
                    await executor.RollbackTranAsync();
                }
                throw;
            }
        }
        else
        {
            var parameters = SqlBuilder.DbParameters;
            return await executor.ExecuteNonQueryAsync(sql, parameters);
        }
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

    public IExpInsert<T> NoParameter()
    {
        return this;
    }

    public IExpInsert<T> ReturnIdentity()
    {
        SqlBuilder.IsReturnIdentity = true;
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

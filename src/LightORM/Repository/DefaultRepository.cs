using LightORM.Extension;
using System.Collections.Concurrent;
using System.Reflection;
using System.Threading;

namespace LightORM.Repository;

internal sealed class DefaultRepository<TEntity> : ILightOrmRepository<TEntity>
{
    private static readonly ParameterExpression parameterExpression = Expression.Parameter(typeof(TEntity), "entity");
    private static readonly Lazy<ConcurrentDictionary<PropertyInfo, PrimaryKeyExpressionBuilder>> keyBuilders = new(CreateKeyBuilders);
    private readonly ISingleScopedExpressionContext context;
    private bool disposedValue;

    public DefaultRepository(IExpressionContext context)
    {
        var tableInfo = TableContext.GetTableInfo<TEntity>();
        var dbKey = tableInfo?.TargetDatabase ?? GetSingleKey(context.Options);
        this.context = context.CreateScoped(dbKey);
    }

    #region 查询操作

    public IQueryable<TEntity> Table => new LightOrmQuery<TEntity>(new LightOrmQueryProvider(context.Ado, typeof(TEntity)));

    public IExpSelect<TEntity> ExpSelect => context.Select<TEntity>();

    public TEntity? GetOne(Expression<Func<TEntity, bool>> predicate)
    {
        return ExpSelect.Where(predicate).First();
    }

    public TEntity? GetOneByKey(object key, string? primaryKey = null)
    {
        var keyWhereExpression = BuildPrimaryKeyExpression(key, primaryKey);
        return GetOne(keyWhereExpression);
    }

    #endregion

    #region 增删改

    public int Insert(TEntity entity)
    {
        context.TryBeginTransaction();
        return context.Insert(entity).Execute();
    }

    public int Update(TEntity entity)
    {
        context.TryBeginTransaction();
        return context.Update(entity).Execute();
    }

    public int Delete(TEntity entity)
    {
        context.TryBeginTransaction();
        return context.Delete(entity).Execute();
    }

    public int Delete(object key, string? primaryKey = null)
    {
        context.TryBeginTransaction();
        var keyWhereExpression = BuildPrimaryKeyExpression(key, primaryKey);
        return context.Delete<TEntity>().Where(keyWhereExpression).Execute();
    }

    public int DeleteFull(bool truncate)
    {
        context.TryBeginTransaction();
        return context.Delete<TEntity>().FullDelete(truncate).Execute();
    }

    #endregion

    #region 批量增删改

    public int InsertRange(IEnumerable<TEntity> entities)
    {
        context.TryBeginTransaction();
        return context.Insert([.. entities]).Execute();
    }
    public int UpdateRange(IEnumerable<TEntity> entities)
    {
        context.TryBeginTransaction();
        return context.Update([.. entities]).Execute();
    }
    public int DeleteRange(IEnumerable<TEntity> entities)
    {
        context.TryBeginTransaction();
        return context.Delete([.. entities]).Execute();
    }

    #endregion

    #region 异步增删改

    public Task<int> InsertAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        context.TryBeginTransaction();
        return context.Insert(entity).ExecuteAsync(cancellationToken);
    }

    public Task<int> UpdateAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        context.TryBeginTransaction();
        return context.Update(entity).ExecuteAsync(cancellationToken);
    }

    public Task<int> DeleteAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        context.TryBeginTransaction();
        return context.Delete(entity).ExecuteAsync(cancellationToken);
    }

    public Task<int> DeleteAsync(object key, string? primaryKey = null, CancellationToken cancellationToken = default)
    {
        context.TryBeginTransaction();
        var keyWhereExpression = BuildPrimaryKeyExpression(key, primaryKey);
        return context.Delete<TEntity>().Where(keyWhereExpression).ExecuteAsync(cancellationToken);
    }

    public Task<int> DeleteFullAsync(bool truncate, CancellationToken cancellationToken = default)
    {
        context.TryBeginTransaction();
        return context.Delete<TEntity>().FullDelete(truncate).ExecuteAsync(cancellationToken);
    }

    #endregion

    #region 异步批量增删改

    public Task<int> InsertRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default)
    {
        context.TryBeginTransaction();
        return context.Insert([.. entities]).ExecuteAsync(cancellationToken);
    }
    public Task<int> UpdateRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default)
    {
        context.TryBeginTransaction();
        return context.Update([.. entities]).ExecuteAsync(cancellationToken);
    }
    public Task<int> DeleteRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default)
    {
        context.TryBeginTransaction();
        return context.Delete([.. entities]).ExecuteAsync(cancellationToken);
    }

    #endregion

    public bool SaveChanges()
    {
        try
        {
            context.TryCommitTransaction();
            return true;
        }
        catch
        {
            context.TryRollbackTransaction();
            throw;
        }
        finally
        {
            context.ResetTransactionState();
        }
    }

    private void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                context?.Dispose();
                context?.TryRollbackTransaction();
            }

            disposedValue = true;
        }
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    #region 私有方法

    private static string GetSingleKey(ExpressionSqlOptions options)
    {
        if (options.DatabaseProviders.Count != 1)
        {
            throw new LightOrmException("无法确定默认数据库，请在LightTableAttribute上指定DatabaseKey");
        }

        return options.DatabaseProviders.First().Key;
    }

    private static Expression<Func<TEntity, bool>> BuildPrimaryKeyExpression(object value, string? primaryKey)
    {
        PrimaryKeyExpressionBuilder builder;
        if (string.IsNullOrEmpty(primaryKey))
        {
            var pkType = value.GetType();
            builder = keyBuilders.Value.FirstOrDefault(k => k.Key.PropertyType == pkType).Value;
            if (builder == null)
            {
                throw new LightOrmException($"未找到类型为{pkType}的主键");
            }
        }
        else
        {
            builder = keyBuilders.Value.FirstOrDefault(k => k.Key.Name == primaryKey).Value;
            if (builder == null)
            {
                throw new LightOrmException($"未找到名称为{primaryKey}的主键");
            }
        }

        return builder.CreatePrimaryKeyExpression(parameterExpression, value);
    }

    private static ConcurrentDictionary<PropertyInfo, PrimaryKeyExpressionBuilder> CreateKeyBuilders()
    {
        var type = typeof(TEntity);
        var tableEntityInfo = TableContext.GetTableInfo(type);
        var dic = new ConcurrentDictionary<PropertyInfo, PrimaryKeyExpressionBuilder>();
        foreach (var primaryKeyColumn in tableEntityInfo.GetPrimaryKeyColumns())
        {
            // 主键条件表达式  entity => entity.PrimaryKey == value
            var name = primaryKeyColumn.PropertyName;
            var property = type.GetProperty(name)!;
            var k = Expression.Property(parameterExpression, property);
            var pkb = new PrimaryKeyExpressionBuilder(k);
            dic.TryAdd(property, pkb);
        }

        return dic;
    }

    private record PrimaryKeyExpressionBuilder(MemberExpression MemberExpression)
    {
        public Expression<Func<TEntity, bool>> CreatePrimaryKeyExpression(ParameterExpression Parameter, object value)
        {
            var body = Expression.Equal(MemberExpression, Expression.Constant(value));
            return Expression.Lambda<Func<TEntity, bool>>(body, Parameter);
        }
    }

    #endregion
}
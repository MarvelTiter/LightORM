using System.Threading;

namespace LightORM.Repository;

public interface ILightOrmRepository<TEntity>: IDisposable
{
    IQueryable<TEntity> Table { get; }
    IExpSelect<TEntity> ExpSelect { get; }
    TEntity? GetOne(Expression<Func<TEntity, bool>> predicate);
    TEntity? GetOneByKey(object key, string? primaryKey = null);
    int Insert(TEntity entity);
    int Update(TEntity entity);
    int Delete(TEntity entity);
    int Delete(object key, string? primaryKey = null);
    int DeleteFull(bool truncate);
    int InsertRange(IEnumerable<TEntity> entities);
    int UpdateRange(IEnumerable<TEntity> entities);
    int DeleteRange(IEnumerable<TEntity> entities);
    bool SaveChanges();
    Task<int> InsertAsync(TEntity entity, CancellationToken cancellationToken = default);
    Task<int> UpdateAsync(TEntity entity, CancellationToken cancellationToken = default);
    Task<int> DeleteAsync(TEntity entity, CancellationToken cancellationToken = default);
    Task<int> DeleteAsync(object key, string? primaryKey = null, CancellationToken cancellationToken = default);
    Task<int> DeleteFullAsync(bool truncate, CancellationToken cancellationToken = default);
    Task<int> InsertRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default);
    Task<int> UpdateRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default);
    Task<int> DeleteRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default);
}

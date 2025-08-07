using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LightORM.Repository;

public interface ILightOrmRepository<TEntity>
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

    Task<int> InsertAsync(TEntity entity);
    Task<int> UpdateAsync(TEntity entity);
    Task<int> DeleteAsync(TEntity entity);
    Task<int> DeleteAsync(object key, string? primaryKey = null);
    Task<int> DeleteFullAsync(bool truncate);
    Task<int> InsertRangeAsync(IEnumerable<TEntity> entities);
    Task<int> UpdateRangeAsync(IEnumerable<TEntity> entities);
    Task<int> DeleteRangeAsync(IEnumerable<TEntity> entities);
}

using System.Threading.Tasks;

namespace LightORM.Repository;

public partial interface IRepository<T>
{
    //IRepository<T> SwitchDatabase(string key);
    int Insert(T item);
    int Update(T item, Expression<Func<T, bool>>? whereExpression);
    int Delete(Expression<Func<T, bool>>? whereExpression);
    int Count(Expression<Func<T, bool>>? whereExpression);
    T? GetSingle(Expression<Func<T, bool>>? whereExpression);
    IEnumerable<T> GetList(Expression<Func<T, bool>>? whereExpression, out long total, int index = 0, int size = 0, Expression<Func<T, object>>? orderByExpression = null, bool asc = true);
    IEnumerable<T> GetList(Expression<Func<T, bool>>? whereExpression, Expression<Func<T, object>>? orderByExpression = null, bool asc = true);

    Task<int> InsertAsync(T item);
    Task<int> UpdateAsync(T item, Expression<Func<T, bool>>? whereExpression);
    Task<int> DeleteAsync(Expression<Func<T, bool>>? whereExpression);
    Task<int> CountAsync(Expression<Func<T, bool>>? whereExpression);
    Task<T?> GetSingleAsync(Expression<Func<T, bool>>? whereExpression);
    Task<IList<T>> GetListAsync(Expression<Func<T, bool>>? whereExpression, out long total, int index = 0, int size = 0, Expression<Func<T, object>>? orderByExpression = null, bool asc = true);
    Task<IList<T>> GetListAsync(Expression<Func<T, bool>>? whereExpression, Expression<Func<T, object>>? orderByExpression = null, bool asc = true);

}

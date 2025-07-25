using LightORM.Providers;
namespace LightORM.ExpressionSql;

internal abstract class ExpressionCoreSqlBase
{
    public abstract ISqlExecutor Ado { get; }
    public IExpSelect<T> Select<T>() => new SelectProvider1<T>(Ado);
    public IExpInsert<T> Insert<T>() => CreateInsertProvider<T>();
    //public IExpInsert<T> Insert<T>(T entity) => CreateInsertProvider<T>(entity);
    //public IExpInsert<T> Insert<T>(IEnumerable<T> entities) => CreateInsertProvider<T>(entities);
    public IExpInsert<T> Insert<T>(params T[] entities)
    {
        if (entities.Length == 1)
        {
            return CreateInsertProvider<T>(entities[0]);
        }
        else
        {
            return CreateInsertProvider<T>(entities);
        }
    }
    InsertProvider<T> CreateInsertProvider<T>(T? entity = default) => new(Ado, entity);
    InsertProvider<T> CreateInsertProvider<T>(IEnumerable<T> entities) => new(Ado, entities);

    public IExpUpdate<T> Update<T>() => CreateUpdateProvider<T>();
    //public IExpUpdate<T> Update<T>(T entity) => CreateUpdateProvider<T>(entity);
    //public IExpUpdate<T> Update<T>(IEnumerable<T> entities) => CreateUpdateProvider<T>(entities);
    public IExpUpdate<T> Update<T>(params T[] entities)
    {
        if (entities.Length == 1)
        {
            return CreateUpdateProvider<T>(entities[0]);
        }
        else
        {
            return CreateUpdateProvider<T>(entities);
        }
    }
    UpdateProvider<T> CreateUpdateProvider<T>(T? entity = default) => new(Ado, entity);
    UpdateProvider<T> CreateUpdateProvider<T>(IEnumerable<T> entities) => new(Ado, entities);

    public IExpDelete<T> Delete<T>() => CreateDeleteProvider<T>();
    //public IExpDelete<T> Delete<T>(bool force, bool truncate = false)
    //{
    //    var provider = CreateDeleteProvider<T>();
    //    provider.ForceDelete = force;
    //    provider.Truncate = truncate;
    //    return provider;
    //}
    //public IExpDelete<T> Delete<T>(T entity) => CreateDeleteProvider<T>(entity);
    //public IExpDelete<T> Delete<T>(IEnumerable<T> entities) => CreateDeleteProvider<T>(entities);
    public IExpDelete<T> Delete<T>(params T[] entities)
    {
        if (entities.Length == 1)
        {
            return CreateDeleteProvider<T>(entities[0]);
        }
        else
        {
            return CreateDeleteProvider<T>(entities);
        }
    }
    DeleteProvider<T> CreateDeleteProvider<T>(T? entity = default) => new(Ado, entity);
    DeleteProvider<T> CreateDeleteProvider<T>(IEnumerable<T> entities) => new(Ado, entities);
}

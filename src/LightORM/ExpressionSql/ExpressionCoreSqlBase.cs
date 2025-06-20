﻿using LightORM.Providers;
namespace LightORM.ExpressionSql;

internal abstract class ExpressionCoreSqlBase
{
    public abstract ISqlExecutor Ado { get; }
    public IExpSelect<T> Select<T>() => new SelectProvider1<T>(Ado);
    public IExpInsert<T> Insert<T>() => CreateInsertProvider<T>();
    public IExpInsert<T> Insert<T>(T entity) => CreateInsertProvider<T>(entity);
    public IExpInsert<T> Insert<T>(IEnumerable<T> entities) => CreateInsertProvider<T>(entities);
    InsertProvider<T> CreateInsertProvider<T>(T? entity = default) => new(Ado, entity);
    InsertProvider<T> CreateInsertProvider<T>(IEnumerable<T> entities) => new(Ado, entities);

    public IExpUpdate<T> Update<T>() => CreateUpdateProvider<T>();
    public IExpUpdate<T> Update<T>(T entity) => CreateUpdateProvider<T>(entity);
    public IExpUpdate<T> Update<T>(IEnumerable<T> entities) => CreateUpdateProvider<T>(entities);
    UpdateProvider<T> CreateUpdateProvider<T>(T? entity = default) => new(Ado, entity);
    UpdateProvider<T> CreateUpdateProvider<T>(IEnumerable<T> entities) => new(Ado, entities);

    public IExpDelete<T> Delete<T>() => CreateDeleteProvider<T>();
    public IExpDelete<T> Delete<T>(T entity) => CreateDeleteProvider<T>(entity);
    public IExpDelete<T> Delete<T>(IEnumerable<T> entities) => CreateDeleteProvider<T>(entities);
    DeleteProvider<T> CreateDeleteProvider<T>(T? entity = default) => new(Ado, entity);
    DeleteProvider<T> CreateDeleteProvider<T>(IEnumerable<T> entities) => new(Ado, entities);
}

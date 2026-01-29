namespace LightORM.Interfaces.ExpSql;

public interface IExpUpdate<T> : ISql<IExpUpdate<T>, T>
{
    IExpUpdate<T> UpdateColumns<TUpdate>(Expression<Func<T, TUpdate>> columns);
    IExpUpdate<T> UpdateColumns(Expression<Func<object>> columns);
    IExpUpdate<T> IgnoreColumns<TIgnore>(Expression<Func<T, TIgnore>> columns);
    IExpUpdate<T> Set<TField>(Expression<Func<T, TField>> exp, TField value);
    IExpUpdate<T> SetNull<TField>(Expression<Func<T, TField>> exp);
    IExpUpdate<T> SetIf<TField>(bool condition, Expression<Func<T, TField>> exp, TField value);
    IExpUpdate<T> SetNullIf<TField>(bool condition, Expression<Func<T, TField>> exp);
}

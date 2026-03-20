namespace LightORM.Interfaces.ExpSql;

public interface IExpUpdate<T> : ISql<IExpUpdate<T>, T>
{
    IExpUpdate<T> UpdateColumns<TUpdate>(Expression<Func<T, TUpdate>> columns);
    IExpUpdate<T> UpdateByName(string propertyName, object? value = null);
    IExpUpdate<T> UpdateByNames(string[] propertyNames, object[]? values = null);
    [Obsolete("这个重载有问题，请使用UpdateColumns<TUpdate>(Expression<Func<T, TUpdate>> columns)")]
    IExpUpdate<T> UpdateColumns(Expression<Func<object>> columns);
    IExpUpdate<T> IgnoreColumns<TIgnore>(Expression<Func<T, TIgnore>> columns);

    IExpUpdate<T> WithVersion<TVersion>(Expression<Func<T, TVersion>> versionField, TVersion versionValue);
    IExpUpdate<T> WithVersion(object versionValue);
    IExpUpdate<T> Set(Expression<Func<T, bool>> exp);
    IExpUpdate<T> Set<TField>(Expression<Func<T, TField>> exp, TField value);
    IExpUpdate<T> SetIf<TField>(bool condition, Expression<Func<T, TField>> exp, TField value);
    IExpUpdate<T> SetNull<TNull>(Expression<Func<T, TNull>> exp);
    IExpUpdate<T> SetNullIf<TNull>(bool condition, Expression<Func<T, TNull>> exp);
}

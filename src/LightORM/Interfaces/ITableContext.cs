namespace LightORM;

public interface ITableContext
{
    ITableEntityInfo? GetTableInfo(Type type);
    Action<ITableColumnInfo, object, object?>? GetSetMethod(Type type);
    Func<ITableColumnInfo, object, object?> GetGetMethod(Type type);
}

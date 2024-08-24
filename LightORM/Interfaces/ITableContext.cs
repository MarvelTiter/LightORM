namespace LightORM;

public interface ITableContext
{
    ITableEntityInfo? GetTableInfo(Type type);
}

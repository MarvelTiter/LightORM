namespace LightORM;

public interface ITableContext
{
    ITableEntityInfo this[string name] { get; }
}

namespace LightORM;

public interface IExpTemp
{
    string Id { get; }
    TableInfo ResultTable { get; }
    internal SelectBuilder SqlBuilder { get; }
}
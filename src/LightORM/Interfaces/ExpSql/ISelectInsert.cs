namespace LightORM;

public interface ISelectInsert<T> : ISql
{
    int Execute();
    Task<int> ExecuteAsync();
}
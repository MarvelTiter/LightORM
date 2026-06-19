namespace LightORM;

public interface IExpSelect : ISql
{
    internal bool IsSubQuery { get; set; }
    internal IContext DbContext { get; }
    internal ISqlExecutor Executor { get; }
    internal SelectBuilder SqlBuilder { get; }
}

public interface IExpTemp<TTemp> : IExpTemp
{
    //IExpSelect<TTemp> AsSelect(string? alias = null);
}
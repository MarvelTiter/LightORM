namespace LightORM;

public interface IExpSelect : ISql
{
    internal bool IsSubQuery { get; set; }
    internal IContext DbContext { get; }
    internal SqlAdo Executor { get; }
    internal SelectBuilder SqlBuilder { get; }
}

public interface IExpTemp<TTemp> : IExpTemp
{
    //IExpSelect<TTemp> AsSelect(string? alias = null);
}
namespace LightORM;

public readonly struct WindowFnSpecification(Expression expression)
{
    public string Idenfity { get; } = $"{Guid.NewGuid():N}";
    public Expression Expression { get; } = expression;
}

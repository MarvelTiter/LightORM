namespace LightORM;

/// <summary>
/// 窗口函数/开窗函数
/// </summary>
public class WinFn
{
    public static IWindowFunction<T> Lag<T>(T? column)
    {
        return default!;
    }
    public static IWindowFunction<T> Lag<T>(T? column, int offset)
    {
        return default!;
    }
    public static IWindowFunction<T> Lag<T>(T? column, int offset, T? defaultValue)
    {
        return default!;
    }
    public static IWindowFunction<int> RowNumber()
    {
        return default!;
    }
}


public interface IWindowFunction<T>
{
    IWindowFunction<T> PartitionBy(object? column);
    IWindowFunction<T> OrderBy<TColumn>(TColumn? column);
    T Value();
}

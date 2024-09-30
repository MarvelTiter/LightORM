namespace LightORM;

/// <summary>
/// 窗口函数/开窗函数
/// </summary>
public class WinFn
{
    public static IWindowFunction<T> Lag<T>(T? column, int offset = 1, T? defaultValue = default)
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
    IWindowFunction<T> OrderBy(object? column);
    T Value();
}

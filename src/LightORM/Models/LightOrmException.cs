using System.Runtime.CompilerServices;

namespace LightORM;

public class LightOrmException(string message) : Exception(message)
{
    [DoesNotReturn]
    public static void Throw(string message) => throw new LightOrmException(message);
    /// <summary>
    /// Throws an <see cref="LightOrmException"/> if <paramref name="argument"/> is null.
    /// </summary>
    /// <exception cref="LightOrmException"></exception>
    [DoesNotReturn]
    public static void ThrowIfNull(object? argument, string message)
    {
        if (argument is null)
        {
            Throw(message);
        }
    }
}

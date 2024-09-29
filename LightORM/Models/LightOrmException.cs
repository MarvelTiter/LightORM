using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LightORM;

public class LightOrmException : Exception
{
    public LightOrmException(string message) : base(message)
    {

    }
    [DoesNotReturn]
    public static void Throw(string message) => throw new LightOrmException(message);
    /// <summary>
    /// Throws an <see cref="LightOrmException"/> if <paramref name="argument"/> is null.
    /// </summary>
    /// <exception cref="LightOrmException"></exception>
    public static void ThrowIfNull(object? argument, string message)
    {
        if (argument is null)
        {
            Throw(message);
        }
    }
}

#if NET462 || NETSTANDARD2_0
using System.Runtime.CompilerServices;

namespace System
{
    internal static class ExceptionPolyfills
    {
        extension(ArgumentNullException)
        {
            public static void ThrowIfNull([NotNull] object? argument, [CallerArgumentExpression(nameof(argument))] string? paramName = null)
            {
                if (argument is null)
                {
                    Throw(paramName);
                }
            }
        }

        [DoesNotReturn]
        internal static void Throw(string? paramName) =>
           throw new ArgumentNullException(paramName);
    }
}

#endif

namespace System.Runtime.CompilerServices
{
    internal sealed class NotNullAttribute : Attribute
    {
    }

    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = false)]
    internal sealed class CallerArgumentExpressionAttribute(string parameterName) : Attribute
    {
        public string ParameterName { get; } = parameterName;
    }

    internal sealed class DoesNotReturnIfAttribute(bool parameterValue) : Attribute
    {
        public bool ParameterValue { get; } = parameterValue;
    }

    [AttributeUsage(AttributeTargets.Method, Inherited = false)]
    internal sealed class DoesNotReturnAttribute : Attribute { }
}
using System.Diagnostics.CodeAnalysis;

namespace LightORM.Models;

internal record Certificate(string Sql, string DbPrefix,
#if NET8_0_OR_GREATER
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)]
#endif
    Type ParameterType)
{
    //public string ConnectString { get; } = conn;
    //public string Sql { get; } = commandText;
    //public Type ParameterType { get; } = parameterType;

    public override string ToString()
    {
        return $"{DbPrefix}_{Sql}_{ParameterType.FullName}";
    }
}

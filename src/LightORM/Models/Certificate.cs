using System.Diagnostics.CodeAnalysis;

namespace LightORM.Models;

internal record Certificate(string Sql, string DbPrefix,
#if NET8_0_OR_GREATER
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)]
#endif
    Type ParameterType)
{
#if NET8_0_OR_GREATER
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)]
#endif
    public Type ParameterType { get; } = ParameterType;

    public override string ToString()
    {
        return $"{DbPrefix}_{Sql}_{ParameterType.FullName}";
    }
}

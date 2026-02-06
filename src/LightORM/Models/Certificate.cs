namespace LightORM.Models;

internal record Certificate(string Sql, string DbPrefix, Type ParameterType)
{
    //public string ConnectString { get; } = conn;
    //public string Sql { get; } = commandText;
    //public Type ParameterType { get; } = parameterType;

    public override string ToString()
    {
        return $"{DbPrefix}_{Sql}_{ParameterType.FullName}";
    }
}

namespace LightORM.Models;
internal class Certificate
{
    public Certificate(string conn, string commandText, Type parameterType)
    {
        ConnectString = conn;
        Sql = commandText;
        ParameterType = parameterType;
    }
    public string ConnectString { get; }
    public string Sql { get; }
    public Type ParameterType { get; }

    public override string ToString()
    {
        return $"{ConnectString}_{Sql}_{ParameterType.GUID}";
    }
}

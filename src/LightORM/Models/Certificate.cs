using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

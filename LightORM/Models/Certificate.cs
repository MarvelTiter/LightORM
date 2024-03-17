using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LightORM.Models;
public class Certificate
{
    public Certificate(string commandText, Type parameterType)
    {
        Sql = commandText;
        ParameterType = parameterType;
    }

    public string Sql { get; }
    public Type ParameterType { get; }

    public override string ToString()
    {
        return $"{Sql}_{ParameterType.GUID}";
    }
}

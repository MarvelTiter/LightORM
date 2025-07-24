using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LightORM.Models;

internal enum ExpValueType
{
    Null,
    Boolean,
    BooleanReverse,
    Collection,
    Other
}
internal readonly struct DbParameterInfo(string name, object? value, ExpValueType type)
{
    public string Name { get; } = name;
    public object? Value { get; } = value;
    public ExpValueType Type { get; } = type;
}

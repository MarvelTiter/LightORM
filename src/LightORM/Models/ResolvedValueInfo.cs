namespace LightORM.Models;

internal enum ExpValueType
{
    Null,
    Boolean,
    BooleanReverse,
    Collection,
    ConstantNull,
    Other
}
internal readonly record struct ResolvedValueInfo(string Name, object? Value, ExpValueType Type)
{
    //public string Name { get; } = Name;
    //public object? Value { get; } = Value;
    //public ExpValueType Type { get; } = Type;
}

internal readonly record struct ResolvedValueInfoWithoutProperty(string Name, object? Value, ExpValueType Type)
{
}



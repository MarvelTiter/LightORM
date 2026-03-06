namespace LightORM.Models;

internal enum ExpValueType
{
    Null,
    Boolean,
    BooleanReverse,
    Collection,
    Other
}
internal readonly record struct ResolvedValueInfo(string Name, object? Value, ExpValueType Type, string? PropertyName)
{
    //public string Name { get; } = Name;
    //public object? Value { get; } = Value;
    //public ExpValueType Type { get; } = Type;
}

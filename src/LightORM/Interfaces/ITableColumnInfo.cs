using System.Reflection;

namespace LightORM.Interfaces;

public interface ITableColumnInfo
{
    ITableEntityInfo Table { get; }
    string ColumnName { get; }
    string PropertyName { get; }
    string? CustomName { get; }
    bool AutoIncrement { get; }
    bool NotNull { get; }
    int? Length { get; }
    object? Default { get; }
    string? Comment { get; }
    bool CanRead { get; }
    bool CanWrite { get; }
    //PropertyInfo Property { get; }
    //Type PropertyType { get; }
    bool IsNullable { get; }
    bool IsNavigate { get; }
    NavigateInfo? NavigateInfo { get; }
    bool IsNotMapped { get; }
    bool IsPrimaryKey { get; }
    object? GetValue(object target);
    void SetValue(object target, object value);
}


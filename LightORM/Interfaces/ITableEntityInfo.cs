namespace LightORM.Interfaces;

public interface ITableEntityInfo
{
    Type? Type { get; }
    string TableName { get; }
    string? Alias { get; set; }
    bool IsAnonymousType { get; }
    string? CustomName { get; set; }
    string? TargetDatabase { get; }
    string? Description { get; }
    ColumnInfo[] Columns { get; }
    object? GetValue(ColumnInfo col, object target);
    void SetValue(ColumnInfo col, object target, object? value);
}

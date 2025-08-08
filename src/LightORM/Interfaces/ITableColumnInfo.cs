namespace LightORM.Interfaces;

public interface ITableColumnInfo
{
    //ITableEntityInfo Table { get; }
    Type TableType { get; }
    string ColumnName { get; }
    string PropertyName { get; }
    string? CustomName { get; }
    bool AutoIncrement { get; }
    bool NotNull { get; }
    int? Length { get; }
    object? Default { get; }
    string? Comment { get; }
    bool IsVersionColumn { get; }
    bool IsIgnoreUpdate { get; }
    bool CanRead { get; }
    bool CanWrite { get; }
    bool CanInit { get; }
    bool IsNullable { get; }
    bool IsNavigate { get; }
    NavigateInfo? NavigateInfo { get; }
    bool IsNotMapped { get; }
    bool IsPrimaryKey { get; }
    Type? AggregateType { get; }
    string? AggregateProp { get; }
    bool IsAggregated { get; }
    bool IsAggregatedProperty { get; }
    //object? GetValue(object target);
    //void SetValue(object target, object value);
}


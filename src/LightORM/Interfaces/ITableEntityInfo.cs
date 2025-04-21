namespace LightORM.Interfaces;

public interface ITableEntityInfo
{
    Type? Type { get; }
    string TableName { get; }
    //string? Alias { get; set; }
    bool IsAnonymousType { get; }
    bool IsTempTable { get; set; }
    string? CustomName { get; set; }
    string? TargetDatabase { get; }
    string? Description { get; }
    ITableColumnInfo[] Columns { get; }
    //object? GetValue(ITableColumnInfo col, object target);
    //void SetValue(ITableColumnInfo col, object target, object? value);
    //ITableColumnInfo? GetColumn(string name);
}

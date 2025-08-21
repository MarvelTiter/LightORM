using LightORM.Extension;

namespace LightORM.Models;
internal record TableEntity : ITableEntityInfo
{
    public TableEntity(Type type)
    {
        Type = type;
        IsAnonymousType = type.IsAnonymous();
    }
    public TableEntity()
    {

    }
    public Type? Type { get; set; }
    public string TableName => CustomName ?? Type?.Name ?? throw new LightOrmException("获取表名异常");
    public string? Alias { get; set; }
    public bool IsAnonymousType { get; set; }
    public bool IsTempTable { get; set; }
    public string? CustomName { get; set; }
    public string? TargetDatabase { get; set; }
    public string? Description { get; set; }
    public ITableColumnInfo[] Columns { get; set; } = [];

}

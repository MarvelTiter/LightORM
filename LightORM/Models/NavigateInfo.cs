namespace LightORM.Models;

internal sealed record NavigateInfo
{
    public NavigateInfo(Type mainType)
    {
        NavigateType = mainType;
    }
    /// <summary>
    /// 多对多类型
    /// </summary>
    public Type NavigateType { get; }
    /// <summary>
    /// 多对多关联表
    /// </summary>
    public Type? MappingType { get; set; }

    public string? MainName { get; set; }
    public string? SubName { get; set; }

}


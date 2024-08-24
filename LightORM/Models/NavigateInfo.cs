namespace LightORM.Models;

public sealed record NavigateInfo
{
    public NavigateInfo(Type mainType)
    {
        NavigateType = mainType;
    }
    public NavigateInfo(Type mainType, Type? mappingType, string? mainName, string? subName, bool isMulti)
    {
        NavigateType = mainType;
        MappingType = mappingType;
        MainName = mainName;
        SubName = subName;
        IsMultiResult = isMulti;
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
    public bool IsMultiResult { get; set; }
}


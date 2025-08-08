namespace LightORM.Models;

internal record JoinInfo
{
    public JoinInfo() { }
    /// <summary>
    /// 联接类型
    /// </summary>
    public TableLinkType JoinType { get; set; }

    /// <summary>
    /// 条件
    /// </summary>
    public string? Where { get; set; }
    public string? ExpressionId { get; set; }
    public TableInfo? EntityInfo { get; set; }
    public bool IsSubQuery { get; set; }
    public SelectBuilder? SubQuery { get; set; }
    
}

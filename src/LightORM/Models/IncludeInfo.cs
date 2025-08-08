namespace LightORM.Models;

internal class IncludeContext
{
    //public IncludeContext(DbBaseType dbBase)
    //{
    //    DbType = dbBase;
    //}
    //public DbBaseType DbType { get; set; }
    public List<IncludeInfo> Includes { get; set; } = [];
    public IncludeContext? ThenInclude { get; set; }
}

internal class IncludeInfo
{
    public IncludeInfo()
    {

    }
    //public TableInfo? SelectedTable { get; set; }
    public NavigateInfo? NavigateInfo { get; set; }
    public ITableColumnInfo? ParentNavigateColumn { get; set; }
    public ITableColumnInfo? ParentWhereColumn { get; set; }
    public TableInfo? ParentTable { get; set; }
    //public ExpressionResolvedResult? ExpressionResolvedResult { get; set; }
    public Expression? IncludeWhereExpression { get; set; }
    //public SelectBuilder? SqlBuilder { get; set; }
}

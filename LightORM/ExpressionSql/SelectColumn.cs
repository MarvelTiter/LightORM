namespace MDbContext.ExpressionSql;

internal struct SelectColumn
{
    public string? TableAlias { get; set; }
    public string? ColumnName { get; set; }
    public string? ColumnAlias { get; set; }
    public string? ValueName { get; set; }
}

namespace MDbContext.ExpressionSql;

internal class SqlFieldInfo
{
    public string? FieldName { get; set; }
    public string? TableAlias => Table?.Alias;
    public string? FieldAlias { get; set; }
    public string? ParameterName { get; set; }
    public string? Compare { get; set; }
    public TableInfo? Table { get; set; }
}

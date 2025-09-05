namespace LightORM.DbStruct;

public struct ReadedTableColumn
{
    public string? ColumnName { get; set; }
    public string? DataType { get; set; }
    public string? IsPrimaryKey { get; set; }
    public string? IsIdentity { get; set; }
    public string? Nullable { get; set; }
    public string? Comments { get; set; }
    public string? DefaultValue { get; set; }
    public string? Length { get; set; }
}

namespace LightORM.DbStruct;

public struct ReadedTable
{
    public string? TableName { get; set; }
    public IEnumerable<ReadedTableColumn>? Columns { get; set; }
}

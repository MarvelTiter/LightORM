namespace LightORM.DbStruct;

public class ReadedTable
{
    public string? TableName { get; set; }
    public IEnumerable<ReadedTableColumn>? Columns { get; set; }
}

namespace LightORM.DbStruct;

public struct DbIndex
{
    public IEnumerable<string> Columns { get; set; }
    public IndexType DbIndexType { get; set; }
    public bool IsUnique { get; set; }
    public bool IsClustered { get; set; }
    public string? Name { get; set; }
}

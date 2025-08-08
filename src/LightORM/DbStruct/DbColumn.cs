namespace LightORM.DbStruct;

[Flags]
public enum IndexType : int
{
    None = 0,
    Normal = 2,
    Primary = 4,
    Unique = 8,
    ColumnStore = 16,
    FullText = 32,
    Bitmap = 64,
    Reverse = 128,
    BTree = 256,
    Brin = 512,
    Hash = 1024,
    Gin = 2048,
    GiST = 4096,
    SP_GiST = 8192
}
public struct DbColumn
{
    public string Name { get; set; }
    public string PropName { get; set; }
    public bool PrimaryKey { get; set; }
    public bool AutoIncrement { get; set; }
    public bool NotNull { get; set; }
    public int? Length { get; set; }
    public object? Default { get; set; }
    public string? Comment { get; set; }
    public Type DataType { get; set; }
}

public struct DbIndex
{
    public IEnumerable<string> Columns { get; set; }
    public IndexType DbIndexType { get; set; }
    public bool IsUnique { get; set; }
    public bool IsClustered { get; set; }
    public string? Name { get; set; }
}

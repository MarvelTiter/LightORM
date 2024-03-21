using LightORM.DbStruct;
namespace LightORM;

[Obsolete("use LightTableAttribute instead")]
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class TableAttribute : Attribute
{
    public string? Name { get; set; }
}


[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class LightTableAttribute : Attribute
{
    public string? Name { get; set; }
    public string? DatabaseKey { get; set; }
}

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class LightTableIndexAttribute : Attribute
{
    public IList<string>? Indexs { get; set; }
    public IndexType DbIndexType { get; set; }
    public string? Name { get; set; }
    public bool IsUnique { get; set; }
    public bool IsClustered { get; set; }
    public LightTableIndexAttribute(params string[] indexs)
    {
        Indexs = indexs;
    }
}

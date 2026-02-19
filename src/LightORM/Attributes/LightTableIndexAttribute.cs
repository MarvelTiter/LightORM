using LightORM.DbStruct;
namespace LightORM;

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

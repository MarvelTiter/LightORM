namespace LightORM;

[AttributeUsage(AttributeTargets.Property)]
public class LightNavigateAttribute : Attribute
{
    public Type? ManyToMany { get; set; }
    public string? MainName { get; set; }
    public string? SubName { get; set; }

    public LightNavigateAttribute()
    {

    }

    public LightNavigateAttribute(string? mainName, string? subName)
    {
        MainName = mainName;
        SubName = subName;
    }

    /// <summary>
    /// mainName与subName分别对应 主表 - 关联表
    /// </summary>
    /// <param name="manyToMany"></param>
    /// <param name="mainName"></param>
    /// <param name="subName"></param>
    public LightNavigateAttribute(Type? manyToMany, string? mainName, string? subName)
    {
        ManyToMany = manyToMany;
        MainName = mainName;
        SubName = subName;
    }
}

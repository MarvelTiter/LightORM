using LightORM.DbStruct;
namespace LightORM;

[Obsolete("use LightTableAttribute instead")]
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class TableAttribute : Attribute
{
    public string? Name { get; set; }
}

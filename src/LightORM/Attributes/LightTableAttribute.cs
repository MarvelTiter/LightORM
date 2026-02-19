namespace LightORM;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class LightTableAttribute : Attribute
{
    public string? Name { get; set; }
    public string? Schema { get; set; }
    public string? DatabaseKey { get; set; }
}

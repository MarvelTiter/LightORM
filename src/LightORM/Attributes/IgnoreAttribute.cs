namespace LightORM;

[AttributeUsage(AttributeTargets.Property)]
[Obsolete("use LightColumnAttribute instead")]
public class IgnoreAttribute : Attribute
{
}

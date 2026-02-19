namespace LightORM;

[AttributeUsage(AttributeTargets.Property)]
[Obsolete("use LightORMIgnore instead")]
public class IgnoreAttribute : Attribute
{
}

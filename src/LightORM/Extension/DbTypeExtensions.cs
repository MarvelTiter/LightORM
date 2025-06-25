namespace LightORM.Extension;

internal static class DbTypeExtensions
{

    public static string AttachPrefix(this ICustomDatabase database, string name) => $"{database.Prefix}{name}";
    public static string AttachEmphasis(this ICustomDatabase database, string name) => database.Emphasis.Insert(1, name);

    public static ICustomDatabase GetDbCustom(this DbBaseType type)
    {
        if (!ExpressionSqlOptions.Instance.Value.CustomDatabases.TryGetValue(type.Name, out var custom))
        {
            throw new LightOrmException($"{type.Name} 数据库未注册 ICustomDatabase");
        }
        return custom;
    }
}

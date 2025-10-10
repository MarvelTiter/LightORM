namespace LightORM.Extension;

internal static class CustomDatabaseExtensions
{
    public static string AttachPrefix(this ICustomDatabase database, string name) => $"{database.Prefix}{name}";

    public static string AttachEmphasis(this ICustomDatabase database, string name)
    {
        if (database.UseIdentifierQuote || database.IsKeyWord(name))
        {
            return database.Emphasis.Insert(1, name);
        }
        return name;
    }

    public static ICustomDatabase GetDbCustom(this DbBaseType type)
    {
        if (!ExpressionSqlOptions.Instance.Value.CustomDatabases.TryGetValue(type.Name, out var custom))
        {
            throw new LightOrmException($"{type.Name} 数据库未注册 ICustomDatabase");
        }

        return custom;
    }
}
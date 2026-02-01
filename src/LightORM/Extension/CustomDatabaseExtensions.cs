using System.Text;

namespace LightORM.Extension;

internal static class CustomDatabaseExtensions
{

    public static string AttachPrefix(this ICustomDatabase database, string name) => $"{database.Prefix}{name}";

    public static StringBuilder WithPrefix(this StringBuilder sql, string name, ICustomDatabase database)
    {
        sql.Append(database.Prefix);
        sql.Append(name);
        return sql;
    }

    public static string AttachEmphasis(this ICustomDatabase database, string name)
    {
        if (database.UseIdentifierQuote || database.IsKeyWord(name))
        {
            return database.Emphasis.Insert(1, name);
        }
        return name;
    }

    public static StringBuilder AppendEmphasis(this StringBuilder sql, string name, ICustomDatabase database)
    {
        if (database.Emphasis.Length != 2)
        {
            throw new LightOrmException("Emphasis must be exactly 2 characters, e.g., \"[]\" or \"``\".");
        }
        if (database.UseIdentifierQuote || database.IsKeyWord(name))
        {
            sql.Append(database.Emphasis[0]);
            sql.Append(name);
            sql.Append(database.Emphasis[1]);
        }
        else
        {
            sql.Append(name);
        }
        return sql;
    }

    public static StringBuilder AppendJoined(this StringBuilder sql, List<string> values, string separator)
    {
        bool first = true;
        foreach (var item in values)
        {
            if (!first)
            {
                sql.Append(separator);
            }
            first = false;
            sql.Append(item);
        }
        return sql;
    }

    public static StringBuilder AppendTableName(this StringBuilder sql, ICustomDatabase database, TableInfo ti, bool useAlias = true, bool useEmphasis = true)
    {
        if (ti.TableEntityInfo.IsTempTable)
        {
            sql.Append(ti.TableEntityInfo.TableName);
        }
        else
        {
            if (ti.Schema is not null)
            {
                sql.AppendEmphasis(ti.Schema, database).Append('.');
            }
            sql.AppendEmphasis(ti.TableName, database);
        }
        if (useAlias && !string.IsNullOrEmpty(ti.Alias))
        {
            sql.Append(' ').Append(ti.Alias);
        }
        return sql;
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
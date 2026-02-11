using LightORM.Performances;
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
        if (values.Count == 0) return sql;
        sql.Append(values[0]);
        for (int i = 1; i < values.Count; i++)
        {
            sql.Append(separator);
            sql.Append(values[i]);
        }
        return sql;
    }

    public static StringBuilder AppendJoined(this StringBuilder sql, ref SlimList<string> values, string separator)
    {
        for (int i = 0; i < values.Count; i++)
        {
            if (i > 0)
            {
                sql.Append(separator);
            }
            sql.Append(values[i]);
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
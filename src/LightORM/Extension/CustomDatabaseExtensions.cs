using System.Text;

namespace LightORM.Extension;

public static class CustomDatabaseExtensions
{

    extension(IDatabaseAdapter database)
    {
        public string AttachPrefix(string name) => $"{database.Prefix}{name}";

        public string AttachEmphasis(string name)
        {
            var shouldQuoteIdentifier = database.QuoteIdentifiers ?? database.UseIdentifierQuote;
            if (shouldQuoteIdentifier || database.IsKeyWord(name))
            {
                return database.Emphasis.Insert(1, name);
            }
            return name;
        }

        public void HandleBooleanValue(StringBuilder sql, bool value)
        {
            sql.Append(database.FormatBooleanValue(value));
        }

        internal string GetValueExpression(SimpleColumn col)
        {
            if (col.Value is null)
            {
                return "NULL";
            }
            if (col.IsStaticValue)
            {
                var v = FormatStaticValue(database, col.Value);
                return v;
            }
            return database.AttachPrefix(col.ParameterName);
        }

        private string FormatStaticValue(object value)
        {
            return value switch
            {
                // 字符串：用单引号包裹，并转义单引号（基础防护）
                string s => $"'{s.Replace("'", "''")}'",

                // 布尔值：根据 SQL 标准，多数数据库用 1/0，PostgreSQL 用 true/false
                bool b => database.FormatBooleanValue(b),

                // 整数类型
                sbyte or byte or short or ushort or int or uint or long or ulong => $"{value}",

                // 浮点数
                float f => f.ToString(System.Globalization.CultureInfo.InvariantCulture),
                double d => d.ToString(System.Globalization.CultureInfo.InvariantCulture),
                decimal m => m.ToString(System.Globalization.CultureInfo.InvariantCulture),

                // 日期时间（可选支持）
                DateTime dt => $"'{dt:yyyy-MM-dd HH:mm:ss}'",
#if NET6_0_OR_GREATER
                DateOnly dOnly => $"'{dOnly:yyyy-MM-dd}'",
                TimeOnly tOnly => $"'{tOnly:HH:mm:ss}'",
#endif
                // Guid（可选）
                Guid g => $"'{g}'",

                // 不支持的类型：抛出异常或返回 NULL / 占位符
                _ => throw new NotSupportedException($"Static value of type '{value.GetType()}' is not supported in SQL literal generation.")
            };
        }

    }

    extension(StringBuilder sql)
    {
        public StringBuilder WithPrefix(string name, IDatabaseAdapter database)
        {
            sql.Append(database.Prefix);
            sql.Append(name);
            return sql;
        }

        public StringBuilder AppendEmphasis(string name, IDatabaseAdapter database)
        {
            if (database.Emphasis.Length != 2)
            {
                throw new LightOrmException("Emphasis must be exactly 2 characters, e.g., \"[]\" or \"``\".");
            }
            var shouldQuoteIdentifier = database.QuoteIdentifiers ?? database.UseIdentifierQuote;
            if (shouldQuoteIdentifier || database.IsKeyWord(name))
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

        public StringBuilder AppendJoined(List<string> values, string separator)
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

        //public static StringBuilder AppendJoined(this StringBuilder sql, ref SlimList<string> values, string separator)
        //{
        //    for (int i = 0; i < values.Count; i++)
        //    {
        //        if (i > 0)
        //        {
        //            sql.Append(separator);
        //        }
        //        sql.Append(values[i]);
        //    }
        //    return sql;
        //}

        public StringBuilder AppendTableName(IDatabaseAdapter database, TableInfo ti, bool useAlias = true, bool useEmphasis = true)
        {
            if (ti.TableEntityInfo.IsTempTable)
            {
                sql.Append(ti.TableEntityInfo.TableName);
            }
            else
            {
                if (ti.Schema is not null && !string.IsNullOrWhiteSpace(ti.Schema))
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

        public StringBuilder AppendEntryColumns(ICollection<MapEntry> entries)
        {
            foreach (MapEntry entry in entries)
            {
                sql.Append(entry.Column);
                sql.Append(',');
            }
            sql.RemoveLast(1);
            return sql;
        }

        public StringBuilder AppendEntryValues(ICollection<MapEntry> entries)
        {
            foreach (MapEntry entry in entries)
            {
                sql.Append(entry.Value);
                sql.Append(',');
            }
            sql.RemoveLast(1);
            return sql;
        }

        public StringBuilder AppendEntryColumns(ICollection<MapEntry> entries, Func<MapEntry, bool> predicate)
        {
            foreach (MapEntry entry in entries)
            {
                if (!predicate.Invoke(entry))
                    continue;
                sql.Append(entry.Column);
                sql.Append(',');
            }
            sql.RemoveLast(1);
            return sql;
        }

        public StringBuilder AppendEntryValues(ICollection<MapEntry> entries, Func<MapEntry, bool> predicate)
        {
            foreach (MapEntry entry in entries)
            {
                if (!predicate.Invoke(entry))
                    continue;
                sql.Append(entry.Value);
                sql.Append(',');
            }
            sql.RemoveLast(1);
            return sql;
        }
    }

    public static IDatabaseAdapter GetDbCustom(this DbBaseType type)
    {
        if (!ExpressionSqlOptions.Instance.Value.CustomDatabases.TryGetValue(type.Name, out var custom))
        {
            throw new LightOrmException($"{type.Name} 数据库未注册 ICustomDatabase");
        }

        return custom;
    }
}
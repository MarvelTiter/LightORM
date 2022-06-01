using MDbContext.Extension;
using MDbEntity.Attributes;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace MDbContext.NewExpSql
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T">MainTable</typeparam>
    internal class TableContext<T>
    {
        private Dictionary<Type, char> tableAlia;
        private Queue<char> charAlia;

        public DbBaseType DbType { get; private set; }

        public TableContext(DbBaseType baseType)
        {
            DbType = baseType;
            Init();
        }
        private void Init()
        {
            charAlia = new Queue<char>();
            for (int i = 97; i < 123; i++)
            {
                // a - z
                charAlia.Enqueue((char)i);
            }
            if (tableAlia == null) tableAlia = new Dictionary<Type, char>();
            else tableAlia.Clear();
        }
        private bool CheckAssign(Type newType, out Type registedType)
        {
            foreach (var item in tableAlia.Keys)
            {
                if (item.IsAssignableFrom(newType) || newType.IsAssignableFrom(item))
                {
                    registedType = item;
                    return true;
                }
            }
            registedType = null;
            return false;
        }

        public string GetTableAlias(Type tableName)
        {
            if (CheckAssign(tableName, out Type registed))
            {
                return tableAlia[registed].ToString();
            }
            else if (tableAlia.Keys.Contains(tableName))
            {
                return tableAlia[tableName].ToString();
            }
            return "";
        }

        public string GetTableName(bool withAlias, Type t = null)
        {
            if (t == null) t = typeof(T);
            var attrs = t.GetAttribute<TableNameAttribute>();
            var tbName = attrs is null ? t.Name : attrs.TableName;
            if (withAlias)
            {
                return tbName + " " + GetTableAlias(t);
            }
            else
            {
                return tbName;
            }
        }

        public bool SetTableAlias(Type tableName)
        {
            if (!CheckAssign(tableName, out _) && !tableAlia.Keys.Contains(tableName))
            {
                tableAlia.Add(tableName, charAlia.Dequeue());
                return true;
            }
            return false;
        }

        public string GetPrefix()
        {
            switch (DbType)
            {
                case DbBaseType.SqlServer:
                case DbBaseType.Sqlite:
                    return "@";
                case DbBaseType.Oracle:
                    return ":";
                case DbBaseType.MySql:
                    return "?";
                default:
                    return "@";
            }
        }
    }

    internal static class TableLinkTypeEx
    {
        internal static string ToLabel(this TableLinkType self)
        {
            switch (self)
            {
                case TableLinkType.From:
                    return "FROM";
                case TableLinkType.LeftJoin:
                    return "LEFT JOIN";
                case TableLinkType.InnerJoin:
                    return "INNER JOIN";
                case TableLinkType.RightJoin:
                    return "RIGHT JOIN";
                default:
                    throw new ArgumentException($"未知的TableLinkType {self}");
            }
        }
    }
    internal enum TableLinkType
    {
        From,
        LeftJoin,
        InnerJoin,
        RightJoin,
    }

    internal class TableInfo
    {
        public string? TableName { get; set; }
        public string? CsName { get; set; }
        public string? Alias { get; set; }
        public Type? Type { get; set; }
        public TableLinkType TableType { get; set; }
    }
    internal class TableContext : ITableContext
    {
        public DbBaseType DbType { get; private set; }
        ConcurrentDictionary<string, TableInfo> tables = new ConcurrentDictionary<string, TableInfo>();
        public TableContext(DbBaseType baseType)
        {
            DbType = baseType;
        }
        public string GetPrefix()
        {
            switch (DbType)
            {
                case DbBaseType.SqlServer:
                case DbBaseType.Sqlite:
                    return "@";
                case DbBaseType.Oracle:
                    return ":";
                case DbBaseType.MySql:
                    return "?";
                default:
                    return "@";
            }
        }

        public string? GetTableAlias(string csName)
        {
            if (tables.TryGetValue(csName, out var table))
            {
                return table.Alias;
            }
            throw new ArgumentException($"TableInfo[{csName}]不存在");
        }

        public string GetTableName(string csName)
        {
            if (tables.TryGetValue(csName, out var table))
            {
                return table.TableName!;
            }
            throw new ArgumentException($"TableInfo[{csName}]不存在");
        }

        public string? GetTableAlias<T>()
        {
            return GetTableAlias(typeof(T).Name);
        }

        public string GetTableName<T>()
        {
            return GetTableName(typeof(T).Name);
        }

        public TableInfo AddTable(Type table, TableLinkType tableLinkType = TableLinkType.From)
        {
            if (!tables.TryGetValue(table.Name, out var info))
            {
                info = new TableInfo();
                info.CsName = table.Name;
                info.TableName = (Attribute.GetCustomAttribute(table, typeof(TableAttribute)) as TableAttribute)?.Name ?? table.Name;
                info.Alias = $"a{tables.Count}";
                info.TableType = tableLinkType;
                tables[info.CsName] = info;
            }
            else
            {
                info.TableType = tableLinkType;
            }
            return info;
        }
    }
}

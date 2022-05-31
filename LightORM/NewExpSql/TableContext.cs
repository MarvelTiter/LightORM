using MDbContext.Extension;
using MDbEntity.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MDbContext.NewExpSql
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T">MainTable</typeparam>
    internal class TableContext<T> : ITableContext
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

    internal class TableContext : ITableContext
    {
        public DbBaseType DbType { get; private set; }

        public TableContext(DbBaseType baseType)
        {
            DbType = baseType;
        }
        public string GetPrefix()
        {
            throw new NotImplementedException();
        }

        public string GetTableAlias(Type tableName)
        {
            throw new NotImplementedException();
        }

        public string GetTableName(bool withAlias, Type t = null)
        {
            throw new NotImplementedException();
        }

        public bool SetTableAlias(Type tableName)
        {
            throw new NotImplementedException();
        }
    }
}

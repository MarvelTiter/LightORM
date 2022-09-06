using MDbContext.ExpressionSql.DbHandle;
using MDbContext.Extension;
using MDbEntity.Attributes;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace MDbContext.ExpressionSql;

internal static class TableLinkTypeEx
{
    internal static string ToLabel(this TableLinkType self)
    {
        switch (self)
        {
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
    None,
    LeftJoin,
    InnerJoin,
    RightJoin,
}

internal class TableInfo : ICloneable
{
    public string? TableName { get; set; }
    public string? CsName { get; set; }
    public string? Alias { get; set; }
    public Type? Type { get; set; }
    public TableLinkType TableType { get; set; }
    public Dictionary<string, SqlFieldInfo>? Fields { get; set; }
    public SqlFragment? Fragment { get; set; }
    public bool Compare(Type type)
    {
        return Type!.IsAssignableFrom(type)
            || type.IsAssignableFrom(Type)
            || type.Equals(Type);
    }
    public object Clone()
    {
        return new TableInfo()
        {
            TableName = TableName,
            CsName = CsName,
            Alias = Alias,
            Type = Type,
            Fields = Fields
        };
    }
}
internal class TableContext : ITableContext
{
    public DbBaseType DbType { get; private set; }
    ConcurrentDictionary<Type, TableInfo> tables = new ConcurrentDictionary<Type, TableInfo>();
    public IDbHelper DbHandler { get; }
    public TableContext(DbBaseType baseType)
    {
        DbType = baseType;
        DbHandler = DbType.GetDbHelper();
    }
    //{
    //switch (DbType)
    //{
    //    case DbBaseType.SqlServer:
    //    case DbBaseType.Sqlite:
    //        return "@";
    //    case DbBaseType.Oracle:
    //        return ":";
    //    case DbBaseType.MySql:
    //        return "?";
    //    default:
    //        return "@";
    //}
    //}

    public string? GetTableAlias(Type table)
    {
        if (tables.TryGetValue(table, out var t))
        {
            return t.Alias;
        }
        throw new ArgumentException($"TableInfo[{table.Name}]不存在");
    }

    public string GetTableName(Type table)
    {
        if (tables.TryGetValue(table, out var t))
        {
            return t.TableName!;
        }
        throw new ArgumentException($"TableInfo[{table.Name}]不存在");
    }

    public string? GetTableAlias<T>()
    {
        return GetTableAlias(typeof(T));
    }

    public string GetTableName<T>()
    {
        return GetTableName(typeof(T));
    }

    public TableInfo AddTable(Type table, TableLinkType tableLinkType = TableLinkType.None)
    {
        if (!tables.TryGetValue(table, out var info))
        {
            info = new TableInfo();
            info.CsName = table.Name;
            info.TableName = (Attribute.GetCustomAttribute(table, typeof(TableAttribute)) as TableAttribute)?.Name ?? table.Name;
            info.Alias = $"a{tables.Count}";
            info.Type = table;
            info.Fields = InitColumns(info);
            tables[table] = info;
        }
        var n = info.Clone() as TableInfo;
        n!.TableType = tableLinkType;
        return n;
    }

    private Dictionary<string, SqlFieldInfo> InitColumns(TableInfo table)
    {
        Dictionary<string, SqlFieldInfo> info = new Dictionary<string, SqlFieldInfo>();
        var props = table.Type!.GetProperties();
        foreach (var prop in props)
        {
            if (prop.HasAttribute<IgnoreAttribute>()) continue;
            var attr = prop.GetAttribute<ColumnAttribute>();
            var field = new SqlFieldInfo
            {
                FieldAlias = prop.Name,
                FieldName = attr?.Name ?? prop.Name,
                IsPrimaryKey = attr?.PrimaryKey ?? false,
                Table = table
            };
            info.Add(prop.Name, field);
        }
        return info;
    }


}

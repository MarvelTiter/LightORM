using MDbContext.ExpressionSql.DbHandle;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MDbContext.ExpressionSql;
internal partial class SqlContext : ITableContext
{
    SqlFragment? fragment;
    Dictionary<string, object> values = new Dictionary<string, object>();
    //internal Dictionary<string, TableInfo> Tables { get; set; } = new Dictionary<string, TableInfo>();
    internal List<TableInfo> Tables { get; set; } = new List<TableInfo>();
    /// <summary>
    /// 本次构建Sql中的字段
    /// </summary>            
    //List<Dictionary<string, SqlFieldInfo>> allFields = new List<Dictionary<string, SqlFieldInfo>>();
    private readonly ITableContext tableContext;
    public SqlContext(ITableContext tableContext)
    {
        this.tableContext = tableContext;
    }

    public void SetFragment(SqlFragment fragment)
    {
        this.fragment = fragment;
    }
    public SqlFragment GetCurrentFragment() => fragment!;
    public TableInfo MainTable => Tables.First();
    public int Length => fragment?.Length ?? 0;
    /// <summary>
    ///  1 - like ; 2 - left like ; 3 - right like
    /// </summary>
    public int LikeMode { get; set; }
    public bool EndWith(string end) => fragment?.Sql.ToString().EndsWith(end) ?? false;
    public void Insert(int index, string content) => fragment?.Insert(index, content);

    public string AppendDbParameter(object value)
    {
        var name = $"{DbHandler.GetPrefix()}p{values.Count}";
        fragment?.Append(name);
        values[name] = CheckLike(value);
        if (fragment?.Names.Count - fragment?.Values.Count == 1)
        {
            fragment?.Values.Add(name);
        }
        LikeMode = 0;
        return name;
    }

    private object CheckLike(object value)
    {
        if (LikeMode > 0)
        {
            switch (LikeMode)
            {
                case 1:
                    return $"%{value}%";
                case 2:
                    return $"%{value}";
                case 3:
                    return $"{value}%";
                default:
                    throw new ArgumentException("Unknow Like Mode");
            }
        }
        else
        {
            return value;
        }
    }

    public void AddEntityField(string name, object value)
    {
        var valueName = $"{DbHandler.GetPrefix()}p{values.Count}";
        fragment?.Names.Add(name);
        fragment?.Values.Add(valueName);
        values[valueName] = value;
    }

    public void AddFieldName(string name)
    {
        fragment?.Names.Add(name);
    }

    public void AddColumn(SqlFieldInfo info, string valueName) => fragment?.AddColumn(info, valueName);

    public void Append(string sql) => fragment?.Append(sql);
    public void Remove(int index, int count) => fragment?.Remove(index, count);
    //public string? Sql() => @string?.ToString();
    public void Clear() => fragment?.Clear();

    public object GetParameters() => values;

    public SqlFieldInfo GetColumn(Type tbType, string csName)
    {
        //SqlFieldInfo field = null;
        //if (Tables.TryGetValue(tbName, out var tableInfo))
        //{
        //    tableInfo!.Fields?.TryGetValue(csName, out field);
        //}
        var tb = Tables.FirstOrDefault(t => t.Compare(tbType));
        if (tb?.Fields?.TryGetValue(csName, out var field) ?? false)
        {
            return field;
        }
        throw new ArgumentException($"Column not found: {csName}");
    }

    #region ITableContext
    public TableInfo AddTable(Type table, TableLinkType tableLinkType = TableLinkType.None)
    {
        var ti = tableContext.AddTable(table, tableLinkType);
        //Tables[table.Name] = ti;
        Tables.Add(ti);
        return ti;
    }

    public string? GetTableAlias(Type csName)
    {
        return tableContext.GetTableAlias(csName);
    }

    public string GetTableName(Type csName)
    {
        return tableContext.GetTableName(csName);
    }

    public string? GetTableAlias<T>()
    {
        return tableContext.GetTableAlias<T>();
    }

    public string GetTableName<T>()
    {
        return tableContext.GetTableName<T>();
    }

    public IDbHelper DbHandler => tableContext.DbHandler;


    #endregion
    public override string ToString()
    {
        return "\n" + paramString();

        string paramString()
        {
            StringBuilder sb = new StringBuilder();
            foreach (KeyValuePair<string, object> item in values)
            {
                sb.AppendLine($"[{item.Key},{item.Value}]");
            }
            return sb.ToString();
        }
    }


    public static SqlContext operator +(SqlContext self, string sql)
    {
        self.Append(sql);
        return self;
    }
    public static SqlContext operator -(SqlContext self, string sql)
    {
        var start = self.Length - sql.Length;
        self.Remove(start, sql.Length);
        return self;
    }

}

﻿using System.Collections.Generic;
using System.Text;

namespace MDbContext.ExpressionSql;

internal class SqlFragment
{
    public StringBuilder Sql { get; set; } = new StringBuilder();
    public List<string> Names { get; set; } = new List<string>();
    public List<string> Values { get; set; } = new List<string>();
    public Dictionary<string, SelectColumn> Columns { get; set; } = new Dictionary<string, SelectColumn>();
    public int Length => Sql.Length;
    public StringBuilder Append(string content) => Sql.Append(content);
    public StringBuilder Clear() => Sql.Clear();
    public StringBuilder Remove(int startIndex, int length) => Sql.Remove(startIndex, length);
    public StringBuilder Insert(int index, string content) => Sql.Insert(index, content);
    public override string ToString()
    {
        return Sql.ToString();
    }

    public bool Has(string name)
    {
        return Names.Contains(name);
    }

    public SqlFragment AddColumn(SqlFieldInfo info, string val)
    {
        Columns.Add(info.FieldAlias ?? info.FieldName!, new SelectColumn
        {
            TableAlias = info.TableAlias,
            ColumnName = info.FieldName,
            ColumnAlias = info.FieldAlias,
            ValueName = val
        });
        return this;
    }
}
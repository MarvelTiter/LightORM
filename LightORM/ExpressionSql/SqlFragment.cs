using System.Collections.Generic;
using System.Text;

namespace LightORM.ExpressionSql;

internal class SqlFragment
{
    public StringBuilder Sql { get; set; } = new StringBuilder();
    public List<string> Names { get; set; } = new List<string>();
    public List<string> Values { get; set; } = new List<string>();
    public Dictionary<string, FieldCell> Columns { get; set; } = new Dictionary<string, FieldCell>();
    public List<UnitCell> Cells { get; set; } = new List<UnitCell>();
    public int Length => Sql.Length;
    public StringBuilder Append(string content) => Sql.Append(content);
    public StringBuilder Clear() => Sql.Clear();
    public StringBuilder Remove(int startIndex, int length) => Sql.Remove(startIndex, length);
    public StringBuilder Insert(int index, string content) => Sql.Insert(index, content);
    public void RemoveLastComma() => Sql.Remove(Length - 2, 2);
    public override string ToString()
    {
        return Sql.ToString();
    }

    public bool Has(string name)
    {
        return Names.Contains(name);
    }

    public void AddCell(UnitCell cell) => Cells.Add(cell);

    public SqlFragment AddColumn(SqlFieldInfo info, string val)
    {
        var key = info.FieldAlias ?? info.FieldName!;
        if (!Columns.ContainsKey(key))
            Columns.Add(key, new FieldCell
            {
                TableAlias = info.TableAlias,
                ColumnName = info.FieldName,
                ColumnAlias = info.FieldAlias,
                ValueName = val
            });
        return this;
    }
}

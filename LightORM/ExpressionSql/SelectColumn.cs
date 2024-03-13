using System.Collections.Generic;

namespace LightORM.ExpressionSql;

internal interface ISqlFieldCell
{
    string Format();
}

internal struct FieldCell
{
    public string? TableAlias { get; set; }
    public string? ColumnName { get; set; }
    public string? ColumnAlias { get; set; }
    public string? ValueName { get; set; }
}

internal class UnitCell : ISqlFieldCell
{
    public string? TableAlias { get; set; }
    public string? ColumnName { get; set; }
    public string? ColumnAlias { get; set; }
    public bool IsPrimaryKey { get; set; }
    public string Format()
    {
        throw new System.NotImplementedException();
    }
}

internal class BinaryCell : ISqlFieldCell
{
    public ISqlFieldCell? Left { get; set; }
    public ISqlFieldCell? Right { get; set; }

    public string? Compare { get; set; }

    public string Format()
    {
        throw new System.NotImplementedException();
    }
}

internal class WhereCell : ISqlFieldCell
{
    public string? TableAlias { get; set; }
    public string? ColumnName { get; set; }
    public string? Campare { get; set; }
    public List<string> Values { get; set; } = new List<string>();

    public void AddValue(string paramName)
    {
        Values.Add(paramName);
    }

    public string Format()
    {
        return "";
    }
}

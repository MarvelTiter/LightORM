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





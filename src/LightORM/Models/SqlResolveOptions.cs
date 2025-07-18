using System;

namespace LightORM;

public enum SqlAction
{
    None,
    Select,
    Update,
    Insert,
    Delete,
}

public enum SqlPartial
{
    None,
    Select,
    SelectFunc,
    Insert,
    Update,
    Delete,
    UpdatePartial,
    Ignore,
    Join,
    Where,
    GroupBy,
    OrderBy,
    Having,
}
public record SqlResolveOptions(SqlAction SqlAction, SqlPartial SqlType)
{
    internal bool UseColumnAlias { get; set; } = true;
    internal bool UseTableAlias { get; set; } = true;
    public bool RequiredColumnAlias => UseColumnAlias && SqlType == SqlPartial.Select;
    public bool RequiredTableAlias => UseTableAlias && SqlAction == SqlAction.Select
        && (SqlType == SqlPartial.Select
        || SqlType == SqlPartial.SelectFunc
        || SqlType == SqlPartial.GroupBy
        || SqlType == SqlPartial.OrderBy
        || SqlType == SqlPartial.Where
        || SqlType == SqlPartial.Join
        || SqlType == SqlPartial.Having);
    public bool Parameterized { get; set; } = true;
    public int ParameterIndex { get; set; }
    //public SqlAction SqlAction { get; set; }
    //public SqlPartial SqlType { get; private set; }
    //public DbBaseType DbType { get; set; }


    public static SqlResolveOptions Select { get; } = new SqlResolveOptions(SqlAction.Select, SqlPartial.Select);
    public static SqlResolveOptions SelectFunc { get; } = new SqlResolveOptions(SqlAction.Select, SqlPartial.SelectFunc);
    public static SqlResolveOptions Group { get; } = new SqlResolveOptions(SqlAction.Select, SqlPartial.GroupBy);
    public static SqlResolveOptions Order { get; } = new SqlResolveOptions(SqlAction.Select, SqlPartial.OrderBy);
    public static SqlResolveOptions Join { get; } = new SqlResolveOptions(SqlAction.Select, SqlPartial.Join);
    public static SqlResolveOptions Where { get; } = new SqlResolveOptions(SqlAction.Select, SqlPartial.Where);
    public static SqlResolveOptions Insert { get; } = new SqlResolveOptions(SqlAction.Insert, SqlPartial.Insert);
    public static SqlResolveOptions Update { get; } = new SqlResolveOptions(SqlAction.Update, SqlPartial.Update);
    public static SqlResolveOptions Having { get; } = new SqlResolveOptions(SqlAction.Select, SqlPartial.Having);
    public static SqlResolveOptions Delete { get; } = new SqlResolveOptions(SqlAction.Delete, SqlPartial.Delete);
    public static SqlResolveOptions UpdateWhere { get; } = new SqlResolveOptions(SqlAction.Update, SqlPartial.Where);
    public static SqlResolveOptions DeleteWhere { get; } = new SqlResolveOptions(SqlAction.Delete, SqlPartial.Where);
    public static SqlResolveOptions UpdateIgnore { get; } = new SqlResolveOptions(SqlAction.Update, SqlPartial.Ignore);
    public static SqlResolveOptions InsertIgnore { get; } = new SqlResolveOptions(SqlAction.Insert, SqlPartial.Ignore);
}
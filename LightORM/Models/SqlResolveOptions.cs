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
public class SqlResolveOptions
{
    public bool RequiredColumnAlias => SqlType == SqlPartial.Select;
    public bool RequiredTableAlias => SqlAction == SqlAction.Select
        && (SqlType == SqlPartial.Select
        || SqlType == SqlPartial.SelectFunc
        || SqlType == SqlPartial.GroupBy
        || SqlType == SqlPartial.OrderBy
        || SqlType == SqlPartial.Where
        || SqlType == SqlPartial.Join);
    public int ParameterIndex { get; set; }
    public SqlAction SqlAction { get; set; }
    public SqlPartial SqlType { get; private set; }
    public DbBaseType DbType { get; set; }


    public static SqlResolveOptions Select { get; } = new SqlResolveOptions() { SqlAction = SqlAction.Select, SqlType = SqlPartial.Select };
    public static SqlResolveOptions SelectFunc { get; } = new SqlResolveOptions() { SqlAction = SqlAction.Select, SqlType = SqlPartial.SelectFunc };
    public static SqlResolveOptions Group { get; } = new SqlResolveOptions() { SqlAction = SqlAction.Select, SqlType = SqlPartial.GroupBy };
    public static SqlResolveOptions Order { get; } = new SqlResolveOptions() { SqlAction = SqlAction.Select, SqlType = SqlPartial.OrderBy };
    public static SqlResolveOptions Join { get; } = new SqlResolveOptions() { SqlAction = SqlAction.Select, SqlType = SqlPartial.Join };
    public static SqlResolveOptions Where { get; } = new SqlResolveOptions() { SqlAction = SqlAction.Select, SqlType = SqlPartial.Where };
    public static SqlResolveOptions Insert { get; } = new SqlResolveOptions() { SqlAction = SqlAction.Insert, SqlType = SqlPartial.Insert };
    public static SqlResolveOptions Update { get; } = new SqlResolveOptions() { SqlAction = SqlAction.Update, SqlType = SqlPartial.Update };
    public static SqlResolveOptions Having { get; } = new SqlResolveOptions() { SqlAction = SqlAction.Update, SqlType = SqlPartial.Having };
    public static SqlResolveOptions Delete { get; } = new SqlResolveOptions() { SqlAction = SqlAction.Delete, SqlType = SqlPartial.Delete };
    public static SqlResolveOptions UpdateWhere { get; } = new SqlResolveOptions() { SqlAction = SqlAction.Update, SqlType = SqlPartial.Where };
    public static SqlResolveOptions DeleteWhere { get; } = new SqlResolveOptions() { SqlAction = SqlAction.Delete, SqlType = SqlPartial.Where };
    public static SqlResolveOptions UpdateIgnore { get; } = new SqlResolveOptions() { SqlAction = SqlAction.Update, SqlType = SqlPartial.Ignore };
    public static SqlResolveOptions InsertIgnore { get; } = new SqlResolveOptions() { SqlAction = SqlAction.Insert, SqlType = SqlPartial.Ignore };
}
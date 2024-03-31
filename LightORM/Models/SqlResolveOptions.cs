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


    public static SqlResolveOptions Select = new SqlResolveOptions() { SqlAction = SqlAction.Select, SqlType = SqlPartial.Select };
    public static SqlResolveOptions SelectFunc = new SqlResolveOptions() { SqlAction = SqlAction.Select, SqlType = SqlPartial.SelectFunc };
    public static SqlResolveOptions Group = new SqlResolveOptions() { SqlAction = SqlAction.Select, SqlType = SqlPartial.GroupBy };
    public static SqlResolveOptions Order = new SqlResolveOptions() { SqlAction = SqlAction.Select, SqlType = SqlPartial.OrderBy };
    public static SqlResolveOptions Join = new SqlResolveOptions() { SqlAction = SqlAction.Select, SqlType = SqlPartial.Join };
    public static SqlResolveOptions Where = new SqlResolveOptions() { SqlAction = SqlAction.Select, SqlType = SqlPartial.Where };
    public static SqlResolveOptions Insert = new SqlResolveOptions() { SqlAction = SqlAction.Insert, SqlType = SqlPartial.Insert };
    public static SqlResolveOptions Update = new SqlResolveOptions() { SqlAction = SqlAction.Update, SqlType = SqlPartial.Update };
    public static SqlResolveOptions UpdatePartial = new SqlResolveOptions() { SqlAction = SqlAction.Update, SqlType = SqlPartial.UpdatePartial };
    public static SqlResolveOptions Delete = new SqlResolveOptions() { SqlAction = SqlAction.Delete, SqlType = SqlPartial.Delete };
    public static SqlResolveOptions UpdateWhere = new SqlResolveOptions() { SqlAction = SqlAction.Update, SqlType = SqlPartial.Where };
    public static SqlResolveOptions DeleteWhere = new SqlResolveOptions() { SqlAction = SqlAction.Delete, SqlType = SqlPartial.Where };
    public static SqlResolveOptions UpdateIgnore = new SqlResolveOptions() { SqlAction = SqlAction.Update, SqlType = SqlPartial.Ignore };
    public static SqlResolveOptions InsertIgnore = new SqlResolveOptions() { SqlAction = SqlAction.Insert, SqlType = SqlPartial.Ignore };
}
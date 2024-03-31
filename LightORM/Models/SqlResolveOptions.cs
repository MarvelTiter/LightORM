using System;

namespace LightORM;

public enum SqlPartial
{
    Default,
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
    Max,
    Min,
    Count,
    Sum,
    Paging,
}
public class SqlResolveOptions
{
    public bool RequiredColumnAlias => SqlType == SqlPartial.Select;
    public bool RequiredTableAlias => SqlType == SqlPartial.Select
        || SqlType == SqlPartial.SelectFunc
        || SqlType == SqlPartial.GroupBy
        || SqlType == SqlPartial.OrderBy
        || SqlType == SqlPartial.Where
        || SqlType == SqlPartial.Join;
    public int ParameterIndex { get; set; }
    public SqlPartial SqlType { get; private set; }
    public DbBaseType DbType { get; set; }

    public static SqlResolveOptions Select = new SqlResolveOptions() { SqlType = SqlPartial.Select };
    public static SqlResolveOptions SelectFunc = new SqlResolveOptions() { SqlType = SqlPartial.SelectFunc };
    public static SqlResolveOptions Group = new SqlResolveOptions() { SqlType = SqlPartial.GroupBy };
    public static SqlResolveOptions Order = new SqlResolveOptions() { SqlType = SqlPartial.OrderBy };
    public static SqlResolveOptions Join = new SqlResolveOptions() { SqlType = SqlPartial.Join };
    public static SqlResolveOptions Where = new SqlResolveOptions() { SqlType = SqlPartial.Where };
    public static SqlResolveOptions Insert = new SqlResolveOptions() { SqlType = SqlPartial.Insert };
    public static SqlResolveOptions Update = new SqlResolveOptions() { SqlType = SqlPartial.Update };
    public static SqlResolveOptions UpdatePartial = new SqlResolveOptions() { SqlType = SqlPartial.UpdatePartial };
    public static SqlResolveOptions Delete = new SqlResolveOptions() { SqlType = SqlPartial.Delete };
    public static SqlResolveOptions UpdateWhere = new SqlResolveOptions() { SqlType = SqlPartial.Where };
    public static SqlResolveOptions DeleteWhere = new SqlResolveOptions() { SqlType = SqlPartial.Where };
    public static SqlResolveOptions UpdateIgnore = new SqlResolveOptions() { SqlType = SqlPartial.Ignore };
    public static SqlResolveOptions InsertIgnore = new SqlResolveOptions() { SqlType = SqlPartial.Ignore };
}
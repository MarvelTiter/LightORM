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
    public bool RequiredColumnAlias { get; private set; }
    public bool RequiredTableAlias { get; private set; }
    public bool IsUnitColumn => SqlType == SqlPartial.Select || SqlType == SqlPartial.GroupBy || SqlType == SqlPartial.OrderBy;
    /// <summary>
    /// 是否需要列标注（防止关键字冲突
    /// </summary>
    public bool RequiredEmphasis { get; set; } = true;
    public bool RequiredResolveEntity => SqlType == SqlPartial.Update || SqlType == SqlPartial.Insert || SqlType == SqlPartial.Delete;
    public bool RequiredComma { get; private set; }
    public int ParameterIndex { get; set; }
    public SqlPartial SqlType { get; private set; }
    public DbBaseType DbType { get; set; }
    

    public static SqlResolveOptions Select = new SqlResolveOptions() { RequiredColumnAlias = true, RequiredTableAlias = true, RequiredComma = true, SqlType = SqlPartial.Select };
    public static SqlResolveOptions SelectFunc = new SqlResolveOptions() { RequiredTableAlias = true, SqlType = SqlPartial.SelectFunc };
    public static SqlResolveOptions Group = new SqlResolveOptions() { RequiredTableAlias = true, RequiredComma = true, SqlType = SqlPartial.GroupBy };
    public static SqlResolveOptions Order = new SqlResolveOptions() { RequiredTableAlias = true, RequiredComma = true, SqlType = SqlPartial.OrderBy };
    public static SqlResolveOptions Join = new SqlResolveOptions() { RequiredTableAlias = true, SqlType = SqlPartial.Join };
    public static SqlResolveOptions Where = new SqlResolveOptions() { RequiredTableAlias = true, SqlType = SqlPartial.Where };
    public static SqlResolveOptions Insert = new SqlResolveOptions() { SqlType = SqlPartial.Insert };
    public static SqlResolveOptions Update = new SqlResolveOptions() { SqlType = SqlPartial.Update };
    public static SqlResolveOptions UpdatePartial = new SqlResolveOptions() { RequiredEmphasis = false, SqlType = SqlPartial.UpdatePartial };
    public static SqlResolveOptions Delete = new SqlResolveOptions() { SqlType = SqlPartial.Delete };
    public static SqlResolveOptions UpdateWhere = new SqlResolveOptions() { SqlType = SqlPartial.Where };
    public static SqlResolveOptions DeleteWhere = new SqlResolveOptions() { SqlType = SqlPartial.Where };
    public static SqlResolveOptions UpdateIgnore = new SqlResolveOptions() { RequiredEmphasis = false, SqlType = SqlPartial.Ignore };
    public static SqlResolveOptions InsertIgnore = new SqlResolveOptions() { RequiredEmphasis = false, SqlType = SqlPartial.Ignore };
}
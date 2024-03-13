using System;

namespace LightORM.ExpressionSql;

public enum BinaryPosition
{
    Left,
    Right,
}
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
public class SqlConfig
{
    public bool RequiredColumnAlias { get; private set; }
    public bool RequiredTableAlias { get; private set; }
    public bool IsUnitColumn => SqlType == SqlPartial.Select || SqlType == SqlPartial.GroupBy || SqlType == SqlPartial.OrderBy;
    public bool RequiredValue => CheckRequiredValue();
    /// <summary>
    /// 是否需要列标注（防止关键字冲突
    /// </summary>
    public bool RequiredEmphasis { get; set; } = true;
    public bool RequiredResolveEntity => SqlType == SqlPartial.Update || SqlType == SqlPartial.Insert || SqlType == SqlPartial.Delete;
    public bool RequiredComma { get; private set; }
    public BinaryPosition BinaryPosition { get; set; }
    public SqlPartial SqlType { get; private set; }
    private bool CheckRequiredValue()
    {
        return BinaryPosition == BinaryPosition.Right &&
            SqlType == SqlPartial.Where ||
            SqlType == SqlPartial.Insert ||
            SqlType == SqlPartial.Update ||
            BinaryPosition == BinaryPosition.Right &&
            SqlType == SqlPartial.SelectFunc
            ;
    }

    public static SqlConfig Select = new SqlConfig() { RequiredColumnAlias = true, RequiredTableAlias = true, RequiredComma = true, SqlType = SqlPartial.Select };
    public static SqlConfig SelectFunc = new SqlConfig() { RequiredTableAlias = true, SqlType = SqlPartial.SelectFunc };
    public static SqlConfig Group = new SqlConfig() { RequiredTableAlias = true, RequiredComma = true, SqlType = SqlPartial.GroupBy };
    public static SqlConfig Order = new SqlConfig() { RequiredTableAlias = true, RequiredComma = true, SqlType = SqlPartial.OrderBy };
    public static SqlConfig Join = new SqlConfig() { RequiredTableAlias = true, SqlType = SqlPartial.Join };
    public static SqlConfig Where = new SqlConfig() { RequiredTableAlias = true, SqlType = SqlPartial.Where };
    public static SqlConfig Insert = new SqlConfig() { SqlType = SqlPartial.Insert };
    public static SqlConfig Update = new SqlConfig() { SqlType = SqlPartial.Update };
    public static SqlConfig UpdatePartial = new SqlConfig() { RequiredEmphasis = false, SqlType = SqlPartial.UpdatePartial };
    public static SqlConfig Delete = new SqlConfig() { SqlType = SqlPartial.Delete };
    public static SqlConfig UpdateWhere = new SqlConfig() { SqlType = SqlPartial.Where };
    public static SqlConfig DeleteWhere = new SqlConfig() { SqlType = SqlPartial.Where };
    public static SqlConfig UpdateIgnore = new SqlConfig() { RequiredEmphasis = false, SqlType = SqlPartial.Ignore };
    public static SqlConfig InsertIgnore = new SqlConfig() { RequiredEmphasis = false, SqlType = SqlPartial.Ignore };
}
namespace MDbContext.NewExpSql
{
    public enum BinaryPosition
    {
        Left,
        Right,
    }
    public enum SqlPartial
    {
        Select,
        Insert,
        Update,
        Delete,
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
        public bool RequiredValue { get; private set; }
        public bool RequiredComma { get; private set; }
        public BinaryPosition BinaryPosition { get; set; }
        public SqlPartial SqlType { get; private set; }
        public static SqlConfig Select = new SqlConfig() { RequiredColumnAlias = true, RequiredTableAlias = true, RequiredComma = true, SqlType = SqlPartial.Select };
        public static SqlConfig Join = new SqlConfig() { RequiredTableAlias = true, SqlType = SqlPartial.Join };
        public static SqlConfig Where = new SqlConfig() { RequiredTableAlias = true, RequiredValue = true, SqlType = SqlPartial.Where };
        public static SqlConfig Insert = new SqlConfig() { RequiredValue = true, SqlType = SqlPartial.Insert };
        public static SqlConfig Update = new SqlConfig() { RequiredValue = true, SqlType = SqlPartial.Update };
    }
}
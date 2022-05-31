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
        public bool RequiredColumnAlias { get; set; }
        public bool RequiredTableAlias { get; set; }
        public bool RequiredValue { get; set; }
        public BinaryPosition BinaryPosition { get; set; }
        public SqlPartial SqlType { get; set; }
        public static SqlConfig Select = new SqlConfig() { RequiredColumnAlias = true, RequiredTableAlias = true };
        public static SqlConfig Join = new SqlConfig() { RequiredTableAlias = true };
        public static SqlConfig Where = new SqlConfig() { RequiredColumnAlias = true, RequiredTableAlias = true, RequiredValue = true };
        public static SqlConfig Insert = new SqlConfig() { RequiredValue = true };
        public static SqlConfig Update = new SqlConfig() { RequiredValue = true };
    }
}
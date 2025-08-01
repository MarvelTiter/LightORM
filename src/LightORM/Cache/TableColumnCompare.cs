namespace LightORM.Cache;
internal static partial class TableContext
{
    public class TableColumnCompare : IEqualityComparer<ITableColumnInfo>
    {
        public static TableColumnCompare Default { get; } = new TableColumnCompare();
        public bool Equals(ITableColumnInfo? x, ITableColumnInfo? y)
        {
            // 处理null值情况
            if (ReferenceEquals(x, y)) return true;
            if (x is null || y is null) return false;

            // 比较表类型和列名
            return x.TableType == y.TableType &&
                   string.Equals(x.ColumnName, y.ColumnName, StringComparison.OrdinalIgnoreCase);
        }

        public int GetHashCode(ITableColumnInfo obj)
        {
            if (obj is null) return 0;

            // 使用表类型和列名的哈希码组合
            unchecked
            {
                int hash = 17;
                hash = hash * 23 + (obj.TableType?.GetHashCode() ?? 0);
                hash = hash * 23 + (obj.ColumnName?.GetHashCode() ?? 0);
                return hash;
            }
        }
    }
}

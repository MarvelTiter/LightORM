namespace LightORM.Utils;

internal class TableColumnInfoEqual : IEqualityComparer<ITableColumnInfo>
{
    public static TableColumnInfoEqual Default { get; } = new TableColumnInfoEqual();
    public bool Equals(ITableColumnInfo? x, ITableColumnInfo? y)
    {
        if (ReferenceEquals(x, y)) return true;
        if (x is null || y is null) return false;
        return x.ColumnType == y.ColumnType && x.TableType == y.TableType && x.PropertyName == y.PropertyName;
    }

    public int GetHashCode(ITableColumnInfo obj)
    {
        if (obj is null) return 0;
#if NET8_0_OR_GREATER
        return HashCode.Combine(obj.ColumnType, obj.TableType, obj.PropertyName);
#else
        //环境不支持 HashCode.Combine，手动计算：
        unchecked
        {
            int hash = 17;
            hash = hash * 23 + (obj.ColumnType?.GetHashCode() ?? 0);
            hash = hash * 23 + (obj.TableType?.GetHashCode() ?? 0);
            hash = hash * 23 + (obj.PropertyName?.GetHashCode() ?? 0);
            return hash;
        }
#endif
    }
}

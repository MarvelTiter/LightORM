namespace LightORM.Extension;

internal static class TableEntityExtensions
{
    extension(TableInfo ti)
    {
        public IEnumerable<ITableColumnInfo> GetNavigateColumns()
        {
            return ti.TableEntityInfo.Columns.Where(c => c.IsNavigate);
        }

        public IEnumerable<ITableColumnInfo> GetNavigateColumns(Func<ITableColumnInfo, bool> predicate)
        {
            return ti.TableEntityInfo.Columns.Where(c => c.IsNavigate && predicate(c));
        }

        public ITableColumnInfo GetColumnInfo(string name)
        {
            return ti.TableEntityInfo.Columns.First(c => c.PropertyName == name);
        }

        public ITableColumnInfo? GetColumn(string columnName)
            => GetColumn(ti.TableEntityInfo, columnName);
    }

    extension(ITableEntityInfo ti)
    {
        public IEnumerable<ITableColumnInfo> GetNavigateColumns()
        {
            return ti.Columns.Where(c => c.IsNavigate);
        }

        public IEnumerable<ITableColumnInfo> GetNavigateColumns(Func<ITableColumnInfo, bool> predicate)
        {
            return ti.Columns.Where(c => c.IsNavigate && predicate(c));
        }

        public IEnumerable<ITableColumnInfo> GetPrimaryKeyColumns()
        {
            return ti.Columns.Where(c => c.IsPrimaryKey);
        }

        public ITableColumnInfo? GetColumn(string columnName)
        {
            for (int i = 0; i < ti.Columns.Length; i++)
            {
                if (ti.Columns[i].PropertyName == columnName)
                    return ti.Columns[i];
            }
            return null;
        }
    }

    //public static ITableColumnInfo GetColumnInfo(this ITableEntityInfo ti, string name)
    //{
    //    return ti.Columns.First(c => c.PropertyName == name);
    //}
}

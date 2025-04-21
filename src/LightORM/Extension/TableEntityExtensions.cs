using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LightORM.Extension
{
    internal static class TableEntityExtensions
    {
        public static IEnumerable<ITableColumnInfo> GetNavigateColumns(this TableInfo ti)
        {
            return ti.TableEntityInfo.Columns.Where(c => c.IsNavigate);
        }

        public static IEnumerable<ITableColumnInfo> GetNavigateColumns(this TableInfo ti, Func<ITableColumnInfo, bool> predicate)
        {
            return ti.TableEntityInfo.Columns.Where(c => c.IsNavigate && predicate(c));
        }

        public static ITableColumnInfo GetColumnInfo(this TableInfo ti, string name)
        {
            return ti.TableEntityInfo.Columns.First(c => c.PropertyName == name);
        }

        public static IEnumerable<ITableColumnInfo> GetNavigateColumns(this ITableEntityInfo ti)
        {
            return ti.Columns.Where(c => c.IsNavigate);
        }

        public static IEnumerable<ITableColumnInfo> GetNavigateColumns(this ITableEntityInfo ti, Func<ITableColumnInfo, bool> predicate)
        {
            return ti.Columns.Where(c => c.IsNavigate && predicate(c));
        }

        public static ITableColumnInfo GetColumnInfo(this ITableEntityInfo ti, string name)
        {
            return ti.Columns.First(c => c.PropertyName == name);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LightORM.Extension
{
    internal static class TableEntityExtensions
    {
        public static IEnumerable<ColumnInfo> GetNavigateColumns(this TableEntity table)
        {
            return table.Columns.Where(c => c.IsNavigate);
        }

        public static IEnumerable<ColumnInfo> GetNavigateColumns(this TableEntity table, Func<ColumnInfo, bool> predicate)
        {
            return table.Columns.Where(c => c.IsNavigate && predicate(c));
        }

        public static ColumnInfo GetColumnInfo(this TableEntity table, string name)
        {
            return table.Columns.First(c => c.PropName == name);
        }
    }
}

using LightORM.Cache;
using LightORM.Extension;
using LightORM.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LightORMTest;

[TestClass]
public class ColumnCompareTest
{
    [TestMethod]
    public void ColumnEqual()
    {
        var tables = new TestTableContext();
        var col1 = tables.GetTableInfo(typeof(User))?.GetColumn(nameof(User.Age));
        var col2 = tables.GetTableInfo(typeof(User))?.GetColumn(nameof(User.Age));
        var equal = TableContext.TableColumnCompare.Default.Equals(col1, col2);
        Assert.IsTrue(equal);
    }
}

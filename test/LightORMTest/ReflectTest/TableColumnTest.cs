using LightORM.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LightORMTest.ReflectTest;

[TestClass]
public class TableColumnTest : TestBase
{
    [TestMethod]
    public void FlatSetterTest()
    {
        var ti = LightORM.Cache.TableContext.GetTableInfo(typeof(SmsLog));
        var col = ti.Columns.First(c => c.PropertyName == nameof(SmsLog.Recive.Code));
        var e = new SmsLog();
        e.Recive.Code = 101;
        var codeValue = col.GetValue(e);
        Assert.IsTrue(codeValue is int i && i == 101);
        col.SetValue(e, 201);
        Assert.IsTrue(e.Recive.Code == 201);
    }
}

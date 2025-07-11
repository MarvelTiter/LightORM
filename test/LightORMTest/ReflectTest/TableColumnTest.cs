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
        var ti = LightORM.Cache.TableContext.GetTableInfo(typeof(UserFlat));
        var col = ti.Columns.First(c => c.PropertyName == nameof(UserFlat.PriInfo.Age));
        var e = new UserFlat();
        e.PriInfo.Age = 101;
        var codeValue = col.GetValue(e);
        Assert.IsTrue(codeValue is int i && i == 101);
        col.SetValue(e, 201);
        Assert.IsTrue(e.PriInfo.Age == 201);
    }
}

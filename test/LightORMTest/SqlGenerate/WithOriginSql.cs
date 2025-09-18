using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LightORMTest.SqlGenerate;

public class WithOriginSql : TestBase
{
    [TestMethod]
    public void WithOriginWhere()
    {
        var sql = Db.Select<User>().Where("age > 10").ToSql();
        Console.WriteLine(sql);
    }
}

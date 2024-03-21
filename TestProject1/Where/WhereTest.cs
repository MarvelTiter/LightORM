using LightORM.Models;
using LightORM.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace TestProject1.Where;

[TestClass]
public class WhereTest : TestBase
{
    [TestMethod]
    public void WhereTrue()
    {
        var p = new Product();
        Expression<Func<Product, bool>> where = e => 1 == 1;
        var result = where.Resolve(SqlResolveOptions.UpdateWhere);
    }

    [TestMethod]
    public void WhereConst()
    {
        var p = new Product();
        Expression<Func<Product, bool>> where = e => e.DeleteMark == true;
        var result = where.Resolve(SqlResolveOptions.UpdateWhere);
    }

    [TestMethod]
    public void WhereEnumerable()
    {
        var nums = Get().ToArray();

        Expression<Func<Product, bool>> where = e => nums.Contains(e.ProductId);
        var option = SqlResolveOptions.UpdateWhere;
        option.DbType = DbBaseType.Sqlite;
        var result = where.Resolve(option);


        IEnumerable<int> Get()
        {
            int i = 0;
            while (i < 10)
            {
                yield return i;
                i++;
            }
        }
    }
}

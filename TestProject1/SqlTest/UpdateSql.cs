using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestProject1.SqlTest
{
    [TestClass]
    public class UpdateSql : TestBase
    {
        [TestMethod]
        public void UpdateEntity()
        {
            var p = new Product();
            var sql = Db.Update(p).ToSql();
            Console.WriteLine(sql);
        }

        [TestMethod]
        public void UpdateColumns()
        {
            var p = new Product();
            var sql = Db.Update<Product>()
                .UpdateColumns(() => new { p.ProductName, p.CategoryId })
                .Where(p => p.ProductId > 10)
                .ToSql();
            Console.WriteLine(sql);
        }

        [TestMethod]
        public void UpdateColumn()
        {
            var p = new Product();
            var sql = Db.Update<Product>()
                .Set(p => p.ProductName, p.ProductName)
                .SetNull(p => p.ProductCode)
                .Where(p => p.ProductId > 10)
                .ToSql();
            Console.WriteLine(sql);
        }

        [TestMethod]
        public void IgnoreColumn()
        {
            var p = new Product();
            var sql = Db.Update(p)
                .IgnoreColumns(p => new { p.ProductName, p.CategoryId })
                .ToSql();
            Console.WriteLine(sql);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestProject1
{
    internal class DataHelper
    {
        static readonly DateTime startDate = new DateTime(1970, 1, 1, 0, 0, 0, 0);
        static long CurrentTimestampSeconds() => Convert.ToInt64((DateTime.UtcNow - startDate).TotalSeconds);
        public static List<Product> GetProductList(int count = 100)
        {
            var list = new List<Product>();
            for (int i = 1; i <= count; i++)
            {
                list.Add(new Product()
                {
                    ProductId = i,
                    CategoryId = 1,
                    ProductCode = $"测试编号_{CurrentTimestampSeconds()}_{i}",
                    ProductName = $"测试名称_{CurrentTimestampSeconds()}_{i}",
                    CreateTime = DateTime.Now,
                    ModifyTime = DateTime.Now,

                });
            }
            return list;
        }
    }
}

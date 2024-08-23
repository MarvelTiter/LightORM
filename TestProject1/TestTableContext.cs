using LightORM.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestProject1
{
    [LightORMTableContext]
    internal partial class TestTableContext : ITableContext
    {
    }

    partial class TestTableContext
    {
        public ITableEntityInfo this[string name]
        {
            get
            {
                switch (name)
                {
                    case "Product":
                        return Product;
                    default:
                        throw new ArgumentException();
                }
            }
        }

        public static global::LightORM.Interfaces.ITableEntityInfo Product => new ProductContext();
    }
}

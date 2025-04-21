using LightORM.Cache;
using LightORM.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestProject1;

[LightORMTableContext]
internal partial class TestTableContext
{
    public void H(Type type)
    {
        if (type == typeof(int) || type.IsAssignableFrom(typeof(int)))
        {

        }
    }
}
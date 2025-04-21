using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LightORM.Extension;

public static class StringBuilderHelper
{
    public static bool EndsWith(this StringBuilder stringBuilder, string ends)
    {
        var el = ends.Length;
        var sl = stringBuilder.Length;
        if (sl < el) return false;
        for (int i = 0; i < el; i++)
        {
            var c1 = stringBuilder[sl - el + i];
            var c2 = ends[i];
            if (c1 != c2)
            {
                return false;
            }
        }
        return true;
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LightORM.Extension;

public static class MethodCallExtensions
{
    public static bool IsWindowFn(this MethodCallExpression? methodCall)
    {
        return methodCall?.Method?.DeclaringType?.Name.StartsWith("IWindowFunction") == true;
    }
}

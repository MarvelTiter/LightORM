using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LightORM;

public class LightOrmException : Exception
{
    public LightOrmException(string message) : base(message)
    {

    }

    //public static void ThrowIf(bool condition, string message)
    //{
    //    if (condition)
    //    {
    //        throw new LightOrmException(message);
    //    }
    //}
}

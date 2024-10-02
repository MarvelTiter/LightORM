using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LightORM;

public class SqlFn
{
    public static int Count<T>(T column)
    {
        return default;
    }

    public static int Count()
    {
        return default;
    }



    public static T Sum<T>(bool exp, T val)
    {
        return default!;
    }

    public static T Sum<T>(T val)
    {
        return default!;
    }

    public static double Avg<T>(bool exp, T val)
    {
        return default;
    }

    public static double Avg<T>(T val)
    {
        return default;
    }

    public static ICaseFragment<T> Case<T>(T? column)
    {
        return default!;
    }
    public static ICaseFragment<T> Case<T>()
    {
        return default!;
    }
}

public interface ICaseFragment<T>
{
    ICaseFragment<T> When(bool condition);
    ICaseFragment<T> Then(T value);
    ICaseFragment<T> Else(T value);
    T End();
}

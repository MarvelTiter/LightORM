namespace LightORM;

public static class SelectSubResultExtensions
{
    public static T Result<T>(this IExpSelect select)
    {
        return default!;
    }

    public static bool Exits(this IExpSelect select)
    {
        return true;
    }

}


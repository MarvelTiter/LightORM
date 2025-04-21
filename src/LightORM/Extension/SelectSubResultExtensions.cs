namespace LightORM;

public static class SelectSubResultExtensions
{
    public static T Result<T>(this IExpSelect _)
    {
        return default!;
    }

    public static bool Exits(this IExpSelect _)
    {
        return true;
    }

}


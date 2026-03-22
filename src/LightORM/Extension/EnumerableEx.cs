using System.Collections;

namespace LightORM.Extension;

internal static class EnumerableEx
{
    public static object? GetValueByIndex(this IEnumerable e, int index)
    {
        return e.Cast<object>().ElementAt(index);
    }

    public static HashSet<ResolvedValueInfoWithoutProperty> RemoveProperty(this HashSet<ResolvedValueInfo> source)
    {
        var newHashset = new HashSet<ResolvedValueInfoWithoutProperty>();
        foreach (var item in source)
        {
            newHashset.Add(new(item.Name, item.Value, item.Type));
        }
        return newHashset;
    }

}

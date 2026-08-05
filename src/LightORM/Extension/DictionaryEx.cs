namespace LightORM.Extension;

internal static class DictionaryEx
{
#if NET462 || NETSTANDARD2_0_OR_GREATER
    public static bool TryAdd<TKey, TValue>(this Dictionary<TKey, TValue> dic, TKey name, TValue value)
    {
        if (!dic.ContainsKey(name))
        {
            dic.Add(name, value);
            return true;
        }
        return false;
    }
#endif
}

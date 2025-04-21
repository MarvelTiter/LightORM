using System;
using System.Collections;
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

    public static void RemoveLast(this StringBuilder stringBuilder, int length)
    {
        stringBuilder.Remove(stringBuilder.Length - length, length);
    }

    public static string Trim(this StringBuilder stringBuilder, params char[] chars)
    {
#if NET462_OR_GREATER
        if (stringBuilder == null)
            throw new ArgumentNullException(nameof(stringBuilder));
#else
        ArgumentNullException.ThrowIfNull(stringBuilder);
#endif
        if (stringBuilder.Length == 0)
            return string.Empty;
        // 如果没有提供自定义字符，则修剪空白字符
        bool trimWhileSpace = chars == null || chars.Length == 0;
        var s = 0;
        var e = stringBuilder.Length - 1;
        // 找到第一个不需要修剪的字符
        while (s <= e)
        {
            char c = stringBuilder[s];
            bool shouldTrim = trimWhileSpace ? char.IsWhiteSpace(c) : Array.IndexOf(chars!, c) >= 0;
            if (!shouldTrim)
                break;
            s++;
        }

        // 找到最后一个不需要修剪的字符
        while (e >= s)
        {
            char c = stringBuilder[e];
            bool shouldTrim = trimWhileSpace ? char.IsWhiteSpace(c) : Array.IndexOf(chars!, c) >= 0;
            if (!shouldTrim)
                break;
            e--;
        }
        return stringBuilder.ToString(s, e - s + 1);
    }
}

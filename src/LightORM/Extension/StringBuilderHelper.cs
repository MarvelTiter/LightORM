using System.Runtime.CompilerServices;
using System.Text;

namespace LightORM.Extension;

public static class StringBuilderHelper
{
    extension(StringBuilder stringBuilder)
    {
        public bool EndsWith(string ends)
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

        public void RemoveLast(int length)
        {
            var startIndex = stringBuilder.Length - length;
            if (startIndex < 0)
                return;
            stringBuilder.Remove(startIndex, length);
        }

        public string Trim(params char[] chars)
        {
#if NET462_OR_GREATER || NETSTANDARD2_0_OR_GREATER
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool CheckEmpty(
#if NET8_0_OR_GREATER
         ReadOnlySpan<char> content
#else
             string content
#endif
            )
        {
#if NET8_0_OR_GREATER
        return content.IsEmpty;
#else
            return string.IsNullOrEmpty(content);
#endif
        }

        public int IndexOf(
#if NET8_0_OR_GREATER
        ReadOnlySpan<char> content
#else
             string content
#endif
            , int startIndex = 0
            , int count = -1)
        {
            if (stringBuilder == null) throw new ArgumentNullException(nameof(stringBuilder));
            if (CheckEmpty(content)) throw new ArgumentNullException(nameof(content));

            int sbLength = stringBuilder.Length;
            int valueLength = content.Length;

            // 参数验证
            if (startIndex < 0 || startIndex > sbLength)
                throw new ArgumentOutOfRangeException(nameof(startIndex));

            if (count < 0)
                count = sbLength - startIndex;

            if (count < 0 || startIndex > sbLength - count)
                throw new ArgumentOutOfRangeException(nameof(count));

            if (valueLength == 0)
                return startIndex; // 空字符串总是匹配

            if (valueLength > count)
                return -1;

            int endIndex = startIndex + count - valueLength;

            char firstChar = content[0];

            for (int i = startIndex; i <= endIndex; i++)
            {
                // 使用StringBuilder的索引器访问字符
                if (stringBuilder[i] != firstChar)
                    continue;

                // 检查剩余字符
                bool match = true;
                for (int j = 1; j < valueLength; j++)
                {
                    if (stringBuilder[i + j] != content[j])
                    {
                        match = false;
                        break;
                    }
                }

                if (match)
                    return i;
            }

            return -1;
        }

        public void ReplaceNull(string placeholder)
        {
            var parameterIndex = stringBuilder.IndexOf(placeholder);

            if (stringBuilder[parameterIndex - 2] == '=')
            {
                //equal
                stringBuilder.Replace($"= {placeholder}", "IS NULL");
            }
            else if (stringBuilder[parameterIndex - 3] == '<' && stringBuilder[parameterIndex - 2] == '>')
            {
                //not equal
                stringBuilder.Replace($"<> {placeholder}", "IS NOT NULL");
            }
            else
            {
                stringBuilder.Replace(placeholder, "NULL");
            }
        }
    }
}

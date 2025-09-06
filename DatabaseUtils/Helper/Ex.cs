using DatabaseUtils.Models;
using System.Text.RegularExpressions;
using LightORM.DbStruct;

namespace DatabaseUtils.Helper;

public static partial class Ex
{
    public static string PascalName(this DatabaseTable self, string? prefix, string? separator)
    {
        return Parse(self.Table.TableName, prefix ?? "", separator ?? "");
    }

    public static string ParseCommentSingleLine(this ReadedTableColumn self)
    {
        if (string.IsNullOrWhiteSpace(self.Comments)) return string.Empty;
        return self.Comments.Replace(Environment.NewLine, " ").Replace("\n", " ");
    }

    public static string PascalName(this ReadedTableColumn self, string? prefix, string? separator)
    {
        return Parse(self.ColumnName, prefix ?? "", separator ?? "");
    }

    private static string Parse(string text, string prefix, string separator)
    {
        var removePrefix = Regex.Replace(text, $"^{prefix}", m => "");

        if (removePrefix.Contains(separator))
        {
            var fix = MatchFirstChar().Replace(removePrefix, m =>
            {
                if (m.Value == separator) return separator;
                else return $"{separator}{m.Value}";
            });
            return Regex.Replace(fix, $"(?:{separator})([A-Za-z0-9]+)", m =>
            {
                var fragment = m.Groups[1].Value;
                return $"{fragment[0..1].ToUpper()}{fragment[1..].ToLower()}";
                //return m.Groups[1].Value;
            });
        }
        else
        {
            //return Regex.Replace(removePrefix, @$"(?<!{separator})([A-Z]+(?=[A-Z][a-z]|$)|[A-Za-z0-9]*)", m =>
            //{
            //    return $"{separator}{m.Value}";
            //});
            return removePrefix;
        }
    }

    [GeneratedRegex("^.")]
    private static partial Regex MatchFirstChar();
}

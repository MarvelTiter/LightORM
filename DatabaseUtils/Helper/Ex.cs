using DatabaseUtils.Models;
using DatabaseUtils.Services;
using DatabaseUtils.Template;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace DatabaseUtils.Helper;

public static partial class Ex
{
    public static string PascalName(this DatabaseTable self, string? prefix, string? separator)
    {
        return Parse(self.TableName, prefix ?? "", separator ?? "");
    }

    public static string ParseCommentSingleLine(this TableColumn self)
    {
        if (string.IsNullOrWhiteSpace(self.Comments)) return string.Empty;
        return self.Comments.Replace(Environment.NewLine, " ").Replace("\n", " ");
    }

    public static string BuildContent(this IDbOperator db, DatabaseTable table, string prefix, string separator)
    {
        var columns = table.Columns;
        var content = new StringBuilder();
        foreach (var item in columns)
        {
            if (!db.ParseDataType(item, out var type))
            {
                continue;
            }
            var lightAttribute = CreateLightColumnAttribute(item, type);
            content.AppendLine(string.Format(ClassTemplate.Property, item.ParseCommentSingleLine(), lightAttribute, type, item.PascalName(prefix, separator)));
        }
        return content.ToString();
    }

    public static string PascalName(this TableColumn self, string? prefix, string? separator)
    {
        return Parse(self.ColumnName, prefix ?? "", separator ?? "");
    }

    public static string CreateLightColumnAttribute(TableColumn column, string csharpType)
    {
        //[LightColumn(Name = ""{1}"")]
        var properties = new StringBuilder($"Name = \"{column.ColumnName}\"");
        if (column.IsPrimaryKey == "YES")
        {
            properties.Append(", PrimaryKey = true");
        }
        if (column.IsIdentity == "YES")
        {
            properties.Append(", AutoIncrement = true");
        }
        if (column.Nullable == "NO")
        {
            properties.Append(", NotNull = true");
        }
        //if (!string.IsNullOrWhiteSpace(column.Length) && csharpType.StartsWith("string"))
        //{
        //    properties.Append($", Length = {column.Length}");
        //}

        if (!string.IsNullOrWhiteSpace(column.Comments))
        {
            properties.Append($", Comment = \"{column.Comments}\"");
        }
        if (!string.IsNullOrWhiteSpace(column.DefaultValue))
        {
            properties.Append($", Default = \"{column.DefaultValue}\"");
        }
        return $"[LightColumn({properties})]";
    }

    public static bool ParseDataType(this IDbOperator db, TableColumn column, out string type)
    {
        return TypeMap.Map(column.DataType, column.Nullable, out type);
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

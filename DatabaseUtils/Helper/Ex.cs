using DatabaseUtils.Models;
using DatabaseUtils.Template;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace DatabaseUtils.Helper
{
    public static class Ex
    {
        public static string PascalName(this DatabaseTable self, string prefix, string separator)
        {
            return Parse(self.TableName, prefix, separator);
        }

        public static string BuildContent(this DatabaseTable self, string prefix, string separator)
        {
            var columns = self.Columns;
            var content = new StringBuilder();
            foreach (var item in columns)
            {
                if (!item.ParseDataType(out var type))
                {
                    continue;
                }
                content.AppendLine(string.Format(ClassTemplate.Property, item.Comments, item.ColumnName, type, item.PascalName(prefix, separator)));
            }
            return content.ToString();
        }

        public static string PascalName(this TableColumn self, string prefix, string separator)
        {
            return Parse(self.ColumnName, prefix, separator);
        }

        public static bool ParseDataType(this TableColumn self, out string type)
        {
            return TypeMap.Map(self.DataType, self.Nullable, out type);
        }

        private static string Parse(string text, string prefix, string separator)
        {
            var removePrefix = Regex.Replace(text.ToLower(), $"^{prefix}", m => "");
            var fix = Regex.Replace(removePrefix, "^.", m =>
            {
                if (m.Value == separator) return separator;
                else return $"{separator}{m.Value}";
            });
            return Regex.Replace(fix, $"({separator})(?<={separator})(\\w)", m =>
            {
                return m.Groups[2].Value.ToUpper();
            });
        }
    }
}

using DatabaseUtils.Helper;
using DatabaseUtils.Models;
using System.Text;
using LightORM.DbStruct;
using LightORM.Interfaces;

namespace DatabaseUtils.Template
{
    public class ClassProperty
    {
        private readonly List<string> attributes = [];
        private readonly ReadedTableColumn column;
        internal ClassProperty(ReadedTableColumn column)
        {
            this.column = column;
        }

        /// <summary>
        /// 使用{0}占位符，自动填充注释
        /// </summary>
        /// <param name="template"></param>
        public void AddAttribute(string template)
        {
            attributes.Add(template);
        }

        private IEnumerable<string> GenAttributes(IDatabaseTableHandler db, Config config, string csharpType, string comment)
        {
            yield return CreateLightColumnAttribute(column, csharpType);
            foreach (var item in attributes)
            {
                if (item.IndexOf("{0}", StringComparison.Ordinal) > -1)
                    yield return string.Format(item, comment);
                else
                    yield return item;
            }
        }

        public string ToString(IDatabaseTableHandler db, Config config)
        {
            if (!db.ParseDataType(column, out var type))
            {
                return
$"""
    /*
        {column.ColumnName}:{column.DataType} -> 未匹配到合适的C#类型
    */
""";
            }
            var comment = column.ParseCommentSingleLine();
            return
$$"""
    /// <summary>
    /// {{comment}}
    /// </summary>
    {{string.Join($"{Environment.NewLine}    ", GenAttributes(db, config, type, comment))}}
    public {{type}} {{column.PascalName(config.Prefix, config.Separator)}} { get; set; }
""";
        }

        private static string CreateLightColumnAttribute(ReadedTableColumn column, string csharpType)
        {
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

            if (!string.IsNullOrWhiteSpace(column.Comments))
            {
                // 注释中的换行/引号会导致生成的字符串字面量跨行或截断，产出的 C# 无法编译
                var comment = column.Comments
                    .Replace(Environment.NewLine, " ")
                    .Replace("\n", " ")
                    .Replace("\r", " ")
                    .Replace("\"", "\\\"");
                properties.Append($", Comment = \"{comment}\"");
            }
            if (!string.IsNullOrWhiteSpace(column.DefaultValue))
            {
                properties.Append($", Default = \"{column.DefaultValue}\"");
            }
            return $"[LightColumn({properties})]";
        }

    }
    public class ClassBuilder
    {
        private readonly DatabaseTable table;
        private readonly string? dbKey;
        public string? ClassName { get; private set; }
        public static ClassBuilder Create(DatabaseTable table, string? dbKey = null) => new(table, dbKey);
        public ClassBuilder(DatabaseTable table, string? dbKey = null)
        {
            this.table = table;
            this.dbKey = dbKey;
        }
        public List<ClassProperty> Properties { get; set; } = [];
        public ClassProperty AddProperty(ReadedTableColumn column)
        {
            var prop = new ClassProperty(column);
            Properties.Add(prop);
            return prop;
        }
        public string ToString(IDatabaseTableHandler db, Config config)
        {
            ClassName = table.PascalName(config.Prefix, config.Separator);
            return
$$"""
/*
 * 该文件由代码生成
 * 时间：{{DateTime.Now:yyyy-MM-dd HH:mm:ss}}
 * 作者：yaoqinglin
 */
using LightORM;
namespace {{config.Namespace}};
[LightTable(Name = "{{table.Table.TableName}}"{{SetDbKey()}})]
public class {{ClassName}}
{
{{string.Join(Environment.NewLine, Properties.Select(p => p.ToString(db, config)))}}
}
""";
        }

        private string SetDbKey()
        {
            if (dbKey is not null)
            {
                return $""", DatabaseKey = "{dbKey}" """;
            }
            return "";
        }
    }
}

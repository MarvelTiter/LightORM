using System.Text.RegularExpressions;
namespace LightORMTest;

public partial class SqlNormalizer
{
    // 标准化 SQL 字符串（移除多余空白、统一格式）
    public static string NormalizeSql(string sql)
    {
        if (string.IsNullOrWhiteSpace(sql))
            return sql;

        // 1. 替换所有连续空白字符（包括换行符、制表符等）为单个空格
        sql = Remove1().Replace(sql, " ");

        // 2. 移除括号、逗号、分号等符号周围的空格
        sql = Remove2().Replace(sql, "$1");

        // 3. 移除首尾空格
        sql = sql.Trim();

        return sql;
    }

    // 比较两个 SQL 是否逻辑相同（忽略空白差异）
    public static bool AreSqlEqual(string sql1, string sql2)
    {
        string normalized1 = NormalizeSql(sql1);
        string normalized2 = NormalizeSql(sql2);
        return normalized1.Equals(normalized2);
    }

    [GeneratedRegex(@"\s+")]
    private static partial Regex Remove1();

    [GeneratedRegex(@"\s*([(),;=])\s*")]
    private static partial Regex Remove2();
}

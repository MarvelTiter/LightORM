using System.Text;
using System.Text.RegularExpressions;

namespace LightORM.Implements;

public abstract class CustomDatabase : ICustomDatabase
{
    public abstract string Prefix { get; }
    public abstract string Emphasis { get; }
    public ISqlMethodResolver MethodResolver { get; }
    public bool UseIdentifierQuote { get; set; } = true;

    private readonly HashSet<string> keyWorks = new(StringComparer.OrdinalIgnoreCase)
    {
        // 数据查询（DQL/DML）
        "SELECT", "FROM", "WHERE", "INSERT", "UPDATE", "DELETE",
        "INTO", "VALUES", "SET",

        // 数据连接与组合
        "JOIN", "INNER", "OUTER", "LEFT", "RIGHT", "FULL", "ON",
        "UNION", "ALL",

        // 数据过滤与分组
        "AND", "OR", "NOT", "IN", "BETWEEN", "LIKE", "IS",
        "GROUP", "BY", "HAVING", "ORDER", "ASC", "DESC", "DISTINCT",

        // 数据定义与结构（DDL）
        "CREATE", "ALTER", "DROP", "TABLE", "DATABASE", "SCHEMA",
        "INDEX", "VIEW", "COLUMN", "CONSTRAINT", "PRIMARY", "FOREIGN",
        "KEY", "UNIQUE", "DEFAULT",

        // 数据类型和函数
        "NULL", "TRUE", "FALSE", "COUNT", "SUM", "AVG", "MAX", "MIN",

        // 事务控制
        "COMMIT", "ROLLBACK", "TRANSACTION",

        // 权限管理
        "GRANT", "REVOKE",

        // 需要特别警惕的"高危"词
        "USER", "DATE", "TIME", "TIMESTAMP",
        "COMMENT", "TYPE", "STATUS", "SESSION", "VALUE"
    };

    protected CustomDatabase(ISqlMethodResolver resolver)
    {
        MethodResolver = resolver;
        // ReSharper disable once VirtualMemberCallInConstructor
        foreach (var keyWord in AddAdditionalKeyWords())
        {
            keyWorks.Add(keyWord);
        }
    }

    protected virtual IEnumerable<string> AddAdditionalKeyWords()
    {
        return [];
    }

    public virtual void Paging(ISelectSqlBuilder builder, StringBuilder sql)
    {
        throw new NotSupportedException();
    }

    public virtual string ReturnIdentitySql()
    {
        throw new NotSupportedException();
    }

    public virtual string HandleBooleanValue(bool value)
    {
        return value ? "1" : "0";
    }

    public virtual string HandleBooleanValueForBulkCopy(bool value)
    {
        return HandleBooleanValue(value);
    }

    public bool IsKeyWord(string keyWork)
    {
        return keyWorks.Contains(keyWork);
    }

    public void AddKeyWord(params string[] keyWords)
    {
        foreach (var keyWord in keyWords)
        {
            keyWorks.Add(keyWord);
        }
    }

    public virtual string HandleMultipleQuerySql(params string[] sqls)
    {
        return string.Join(";", sqls);
    }

    public virtual string RewriteParameterReferences(string sql, string prefix)
    {
        // 将 @param 重写为 @q0_param
        return Regex.Replace(sql, @$"{Prefix}(\w+)", $"{Prefix}{prefix}_$1");
    }

    public virtual string DeleteTemplate => throw new NotImplementedException();
}
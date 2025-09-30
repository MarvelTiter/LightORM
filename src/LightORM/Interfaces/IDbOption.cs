using System.Data.Common;

namespace LightORM;

public interface IDbOption
{
    string? DbKey { get; set; }
    string? MasterConnectionString { get; set; }
    string[]? SalveConnectionStrings { get; set; }
    ISqlMethodResolver MethodResolver { get; }
    void OverrideDbProviderFactory(DbProviderFactory factory);
    /// <summary>
    /// 扩展数据库关键词，对于关键词，会强制使用引用标识符 
    /// </summary>
    /// <param name="keyWords"></param>
    void AddDbKeyWords(params string[] keyWords);
}

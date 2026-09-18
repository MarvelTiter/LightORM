namespace LightORM;

public record TableOptions
{
    public TableOptions()
    {

    }
    /// <summary>
    /// 显式指定数据库版本。一旦指定，则<b>不发起版本探测</b>；同时它优先于探测结果。
    /// </summary>
    public Version? SpecificVersion { get; set; }

    /// <summary>
    /// 是否允许在构造期后台探测数据库版本，默认开启。
    /// <para>
    /// 支持版本差异的方言会把探测结果翻译成自己的能力位；关掉探测（如 ToSql 纯 SQL 生成场景，
    /// 不需要也不应该建连）时按<b>完整功能</b>处理。探测失败同样落到完整功能。
    /// </para>
    /// </summary>
    public bool DetectVersion { get; set; } = true;

    public bool NotCreateIfExists { get; set; }
    public bool UseUnicodeString { get; set; } = true;
    public bool SupportComment { get; set; } = true;
    public JSONBackend JSONBackend { get; set; } = JSONBackend.Text;
    public string? SpecificJsonColumnDbType { get; set; }

    private int defaultStringLength = 256;
    public int DefaultStringLength
    {
        get => UseUnicodeString ? defaultStringLength / 2 : defaultStringLength;
        set => defaultStringLength = value;
    }

   
}

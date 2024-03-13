namespace LightORM.DbStruct;

public class TableGenerateOption
{
    public bool NotCreateIfExists { get; set; }
    public bool UseUnicodeString { get; set; } = true;
    public bool SupportComment { get; set; }

    /// <summary>
    /// 高级版本的Oracle，适用自增列语法 GENERATED ALWAYS AS IDENTITY
    /// </summary>
    public bool OracleOverVersion { get; set; }
    public string? OracleTableSpace { get; set; }
    public string? OracleUserId { get; set; }

    private int defaultStringLength = 256;
    public int DefaultStringLength
    {
        get => UseUnicodeString ? defaultStringLength / 2 : defaultStringLength;
        set => defaultStringLength = value;
    }
}

namespace LightORM;

public record TableOptions
{
    public TableOptions()
    {

    }
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

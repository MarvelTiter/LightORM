namespace LightORM;

internal class ConstString
{
    public const string Main = "MainDb";
    public const string ASC = "ASC";
    public const string DESC = "DESC";
    public const string GROUPING_SETS = "GROUPING_SETS";
}

internal class GroupingSetsFlags
{
    private GroupingSetsFlags()
    {
        
    }
    public static readonly GroupingSetsFlags Instance = new();
}

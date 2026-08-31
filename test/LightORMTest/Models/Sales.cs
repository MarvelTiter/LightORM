namespace LightORMTest.Models;

[LightTable(Name = "SALES")]
public class Sales
{
    [LightColumn(Name = "REGION", PrimaryKey = true)]
    public string? Region { get; set; }
    [LightColumn(Name = "PROVINCE", PrimaryKey = true)]
    public string? Province { get; set; }
    [LightColumn(Name = "PRODUCT")]
    public string? Product { get; set; }
    [LightColumn(Name = "AMOUNT")]
    public int Amount { get; set; }

    [LightColumn(Name = "VERSION", Version = true)]
    public int Version { get; set; }
}

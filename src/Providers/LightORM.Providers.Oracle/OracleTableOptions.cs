namespace LightORM.Providers.Oracle;

public record OracleTableOptions : TableOptions
{

    public string? TableSpace { get; set; }
    public string? UserId { get; set; }
    public int? InitialLONGFetchSize { get; set; }
}

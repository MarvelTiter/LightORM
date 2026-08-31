namespace LightORM.Providers.KingbaseES;

public record KingbaseESTableOptions : TableOptions
{
    public string? TableSpace { get; set; }
}

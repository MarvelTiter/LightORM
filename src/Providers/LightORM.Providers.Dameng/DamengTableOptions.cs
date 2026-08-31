namespace LightORM.Providers.Dameng;

public record DamengTableOptions : TableOptions
{
    public string? TableSpace { get; set; }
    public string? UserId { get; set; }
}

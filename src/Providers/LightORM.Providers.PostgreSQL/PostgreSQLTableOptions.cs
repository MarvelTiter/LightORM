namespace LightORM.Providers.PostgreSQL;

public record PostgreSQLTableOptions: TableOptions
{
    public string? TableSpace { get; set; }
}

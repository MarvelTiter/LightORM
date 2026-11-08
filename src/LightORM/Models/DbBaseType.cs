namespace LightORM;

public record DbBaseType(string Name)
{
    public static DbBaseType Sqlite { get; } = new DbBaseType("Sqlite");
    public static DbBaseType MySql { get; } = new DbBaseType("MySql");
    public static DbBaseType Oracle { get; } = new DbBaseType("Oracle");
    public static DbBaseType SqlServer { get; } = new DbBaseType("SqlServer");
    public static DbBaseType PostgreSQL { get; } = new DbBaseType("PostgreSQL");
}

namespace LightORM;
//public enum DbBaseType
//{
//    SqlServer = 0,
//    SqlServer2012 = 1,
//    Oracle = 2,
//    MySql = 3,
//    Sqlite = 4,
//}

public record DbBaseType(string Name)
{
    public static DbBaseType Sqlite { get; } = new DbBaseType("Sqlite");
    public static DbBaseType MySql { get; } = new DbBaseType("MySql");
    public static DbBaseType Oracle { get; } = new DbBaseType("Oracle");
    public static DbBaseType SqlServer { get; } = new DbBaseType("SqlServer");
    public static DbBaseType PostgreSQL { get; } = new DbBaseType("PostgreSQL");

}

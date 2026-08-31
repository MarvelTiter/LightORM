global using LightORM;
global using LightORM.Interfaces;
global using System.Threading.Tasks;
global using LightORM.Providers.PostgreSQL.Extensions;

public static class ConnectString
{
    public static string Value => "Host=localhost;Port=5432;Database=testdb;Username=postgres;Password=123456;";
}
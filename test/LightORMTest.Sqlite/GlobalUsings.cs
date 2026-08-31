global using LightORM;
global using LightORM.Interfaces;
global using LightORM.Providers.Sqlite.Extensions;

public static class ConnectString
{
    public static string Value => $"DataSource={Path.GetFullPath("../../../../../test.db")}";
}
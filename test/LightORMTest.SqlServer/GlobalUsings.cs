global using LightORM;
global using LightORM.Interfaces;
global using System;
global using LightORM.Providers.SqlServer.Extensions;

public static class ConnectString
{
    public static string Value => $"Server=localhost;Database=TestDb;User ID=sa;Password=Ybeluoek123!@#;TrustServerCertificate=True;";
}
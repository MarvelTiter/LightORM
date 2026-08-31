global using LightORM;
global using LightORM.Interfaces;
global using LightORM.Providers.MySql.Extensions;

namespace LightORMTest.MySql
{
    public static class ConnectString
    {
        public static string Value => $"Server=localhost;port=3306;User ID=root;Password=123456;Database=test_db;CharSet=utf8;pooling=true;SslMode=None;Default Command Timeout=300;AllowLoadLocalInfile=true;";
    }
}
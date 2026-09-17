global using LightORM;
global using LightORM.Interfaces;

namespace LightORMTest.KingbaseES
{
    public static class ConnectString
    {
        public static string Value => "Server=localhost; Port=54321; User Id=kingbase; Password=123456; Database=kingbase;";
    }
}
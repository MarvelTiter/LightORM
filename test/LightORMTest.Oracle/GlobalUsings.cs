global using LightORM;
global using LightORM.Interfaces;
global using System.Collections.Generic;
global using LightORM.Providers.Oracle.Extensions;

namespace LightORMTest.Oracle
{
    public static class ConnectString
    {
        public static string Value => "User Id=lightorm_test;Password=lightorm_test;Data Source=localhost:1521/XE;";
    }
}
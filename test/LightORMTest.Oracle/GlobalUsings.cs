global using LightORM;
global using LightORM.Interfaces;
global using System;
global using System.Collections.Generic;
global using System.Linq;
global using System.Text;
global using System.Threading.Tasks;
global using LightORM.Providers.Oracle.Extensions;

namespace LightORMTest.Oracle
{
    public static class ConnectString
    {
        public static string Value => "User Id=lightorm_test;Password=lightorm_test;Data Source=localhost:1521/XE;";
    }
}
global using LightORM;
global using LightORM.Interfaces;
global using System;
global using System.Collections.Generic;
global using System.Linq;
global using System.Text;
global using System.Threading.Tasks;
global using LightORM.Providers.Dameng.Extensions;

namespace LightORMTest.Dameng
{
    public static class ConnectString
    {
        public static string Value => "Server=localhost; Port=5236; User Id=LIGHTORM_TEST; Password=LIGHTORM_TEST; Database=DAMENG;";
    }
}
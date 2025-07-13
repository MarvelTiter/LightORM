global using LightORM;
global using LightORM.Interfaces;
global using System;
global using System.Collections.Generic;
global using System.Linq;
global using System.Text;
global using System.Threading.Tasks;
global using LightORM.Providers.SqlServer.Extensions;

public static class ConnectString
{
    public static string Value => $"DataSource={Path.GetFullPath("../../../../../test.db")}";
}
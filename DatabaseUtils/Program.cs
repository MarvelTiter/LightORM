﻿using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using LightORM;
namespace DatabaseUtils
{
    [LightORMTableContext]
    public partial class DbContext
    {

    }
    public class Program
    {
        [STAThread]
        public static void Main(string[] args)
        {
            var builder = Host.CreateApplicationBuilder(args);
            builder.Services.AddLightOrm(option =>
            {
                option.SetTableContext(new DbContext());
                option.SetWatcher(aop =>
                {
                    aop.DbLog = (s, p) =>
                    {
                        Console.WriteLine(s);
                    };
                });
            });
            builder.Services.AddAntDesign();
            builder.Services.AddSingleton<App>();
            builder.Services.AddTransient<MainWindow>();
            builder.Services.AddHostedService<WpfHostedService<App, MainWindow>>();
            builder.Services.AddWpfBlazorWebView();

            builder.Build().Run();
        }
    }
}

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using LightORM;
using LightORM.Implements;
using LightORM.Models;

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
                option.SetTableContext<DbContext>();
                option.UseInterceptor<SqlLogger>();
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

public class SqlLogger : AdoInterceptorBase
{
    public override void OnException(SqlExecuteExceptionContext context)
    {
        base.OnException(context);
    }
}

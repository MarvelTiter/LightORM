using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace DatabaseUtils
{
    public class Program
    {
        [STAThread]
        public static void Main(string[] args)
        {
            var builder = Host.CreateApplicationBuilder(args);

            builder.Services.AddAntDesign();
            builder.Services.AddSingleton<App>();
            builder.Services.AddTransient<MainWindow>();
            builder.Services.AddHostedService<WpfHostedService<App, MainWindow>>();
            builder.Services.AddWpfBlazorWebView();

            builder.Build().Run();
        }
    }
}

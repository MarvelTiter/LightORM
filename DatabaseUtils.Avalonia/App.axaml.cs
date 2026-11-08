using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using DatabaseUtils.Avalonia.ViewModels;
using DatabaseUtils.Avalonia.Views;
using LightORM;
using LightORM.Providers.Dameng.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;

namespace DatabaseUtils.Avalonia
{
    public partial class App : Application
    {
        /// <summary>
        /// 全局服务容器，ViewModel 可通过 App.Services 解析服务。
        /// </summary>
        public static IServiceProvider Services { get; private set; } = default!;

        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                // 构建 IConfiguration，加载 db.json。
                // optional: false —— 缺文件时直接启动失败，比运行期抛 KeyNotFoundException 好查。
                var configuration = new ConfigurationBuilder()
                    .AddJsonFile("db.json", optional: false, reloadOnChange: true)
                    .Build();
                // 与 Hybrid 版本保持一致的 LightORM 初始化
                var services = new ServiceCollection();
                services.AddSingleton<IConfiguration>(configuration);
                services.AddLightOrm(option =>
                {
                    option.SetTableContext<DatabaseUtils.DbContext>();
                    option.UseInterceptor<DatabaseUtils.SqlLogger>();
                    // 只登记“从配置创建 Dameng provider”的工厂；实际库由 db.json 提供
                    option.UseDameng();
                });

                Services = services.BuildServiceProvider();

                desktop.MainWindow = new MainWindow
                {
                    DataContext = new MainViewModel(
                        Services.GetRequiredService<IExpressionContext>()),
                };
            }

            base.OnFrameworkInitializationCompleted();
        }
    }
}

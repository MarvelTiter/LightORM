using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DatabaseUtils.Avalonia.Services;
using DatabaseUtils.Models;
using DatabaseUtils.Template;
using LightORM;
using LightORM.Interfaces;
using LightORM.Providers.Dameng;
using LightORM.Providers.MySql;
using LightORM.Providers.Oracle;
using LightORM.Providers.PostgreSQL;
using LightORM.Providers.SqlServer;
using System.Collections.ObjectModel;
using System.IO;

namespace DatabaseUtils.Avalonia.ViewModels;

/// <summary>
/// 主界面 ViewModel，复刻 Hybrid 版本 WpfApp.razor 的全部业务逻辑。
/// </summary>
public partial class MainViewModel : ViewModelBase
{
    private readonly IExpressionContext context;
    private readonly Dictionary<string, Func<string, IDatabaseProvider>> supportedDb = [];
    private IDatabaseProvider? currentDbProvider;
    private IDialogService? dialogService;

    public MainViewModel(IExpressionContext context)
    {
        this.context = context;

        supportedDb.Add("Dameng", str => DamengProvider.Create(o => o.MasterConnectionString = str));
        supportedDb.Add(DbBaseType.Oracle.Name, str => OracleProvider.Create(o => o.MasterConnectionString = str));
        supportedDb.Add(DbBaseType.PostgreSQL.Name, str => PostgreSQLProvider.Create(o => o.MasterConnectionString = str));
        supportedDb.Add(DbBaseType.MySql.Name, str => MySqlProvider.Create(o => o.MasterConnectionString = str));
        supportedDb.Add(DbBaseType.SqlServer.Name, str => SqlServerProvider.Create(SqlServerVersion.V1, o => o.MasterConnectionString = str));
        SupportedDbNames = [.. supportedDb.Keys];

        // 从配置恢复上次的状态（经属性赋值，保持与界面绑定一致）
        LastSelectedDb = Config.LastSelectedDb;
        Connectstring = Config.Connectstring;
    }

    /// <summary>应用配置（JSON 持久化），生成设置直接读写该实例。</summary>
    public Config Config { get; } = new();

    public string[] SupportedDbNames { get; }

    public ObservableCollection<TableItemViewModel> Tables { get; } = [];

    public ObservableCollection<GeneratedTableItem> GeneratedTables { get; } = [];

    public ObservableCollection<AttributeItemViewModel> ExternalAttributes { get; } = [];

    [ObservableProperty]
    public partial string? LastSelectedDb { get; set; }

    [ObservableProperty]
    public partial string? Connectstring { get; set; }

    [ObservableProperty]
    public partial string? DbKey { get; set; }

    /// <summary>全选/取消全选（绑定左侧"全选"CheckBox）。</summary>
    [ObservableProperty]
    public partial bool SelectAll { get; set; }

    partial void OnLastSelectedDbChanged(string? value) => Config.LastSelectedDb = value;

    partial void OnConnectstringChanged(string? value) => Config.Connectstring = value;

    partial void OnSelectAllChanged(bool value)
    {
        foreach (var table in Tables)
        {
            table.IsSelected = value;
        }
    }

    /// <summary>由宿主窗口注入对话框服务（在 OnDataContextChanged 时调用）。</summary>
    public void AttachDialogService(IDialogService service) => dialogService = service;

    [RelayCommand]
    private async Task ShowSettingsAsync()
    {
        if (dialogService is null) return;
        await dialogService.ShowSettingsDialogAsync();
    }

    [RelayCommand]
    private async Task ShowAttributesAsync()
    {
        if (dialogService is null) return;
        await dialogService.ShowAttributesDialogAsync();
    }

    [RelayCommand]
    private async Task ConnectAsync()
    {
        try
        {
            if (string.IsNullOrEmpty(LastSelectedDb) || string.IsNullOrWhiteSpace(Connectstring))
            {
                await ShowMessageAsync("提示", "数据库类型和连接字符串不能为空");
                return;
            }

            if (!supportedDb.TryGetValue(LastSelectedDb, out var factory))
            {
                await ShowMessageAsync("提示", $"不支持的数据库类型 {LastSelectedDb}");
                return;
            }

            currentDbProvider = factory(Connectstring);
            var result = await context.GetTablesAsync(currentDbProvider);
            Tables.Clear();
            foreach (var table in result)
            {
                Tables.Add(new TableItemViewModel(new DatabaseTable(table)));
            }
        }
        catch (Exception ex)
        {
            await ShowExceptionAsync(ex, "连接失败");
        }

    }

    [RelayCommand]
    private async Task BuildAsync()
    {
        if (string.IsNullOrEmpty(Config.Namespace))
        {
            await ShowMessageAsync("提示", "未设置命名空间!");
            return;
        }

        var selected = Tables.Where(t => t.IsSelected).ToArray();
        if (selected.Length == 0)
        {
            await ShowMessageAsync("提示", "未选择表!");
            return;
        }

        ArgumentNullException.ThrowIfNull(currentDbProvider);

        GeneratedTables.Clear();
        foreach (var table in selected)
        {
            try
            {
                table.Table.Table = await context.GetTableStructAsync(currentDbProvider, table.Table.Table);
                var cb = ClassBuilder.Create(table.Table, DbKey);
                foreach (var item in table.Table.Table.Columns ?? [])
                {
                    var prop = cb.AddProperty(item);
                    foreach (var ea in ExternalAttributes)
                    {
                        if (string.IsNullOrEmpty(ea.Value)) continue;
                        prop.AddAttribute(ea.Value);
                    }
                }

                GeneratedTables.Add(new GeneratedTableItem
                {
                    TableName = table.TableName,
                    GeneratedResult = cb.ToString(currentDbProvider.DbHandler, Config),
                    CsFileName = cb.ClassName,
                });
            }
            catch (Exception ex)
            {
                await ShowExceptionAsync(ex, "生成失败");
            }
        }
    }

    [RelayCommand]
    private async Task SaveToLocalAsync()
    {
        if (string.IsNullOrEmpty(Config.SavedPath))
        {
            await ShowMessageAsync("提示", "保存路径为空!");
            return;
        }

        try
        {
            if (!Directory.Exists(Config.SavedPath))
            {
                Directory.CreateDirectory(Config.SavedPath);
            }

            foreach (var item in GeneratedTables)
            {
                if (string.IsNullOrEmpty(item.GeneratedResult)) continue;
                var path = Path.Combine(Config.SavedPath, $"{item.CsFileName}.cs");
                if (File.Exists(path))
                {
                    File.Delete(path);
                }

                await File.WriteAllTextAsync(path, item.GeneratedResult);
            }

            await ShowMessageAsync("提示", $"已保存 {GeneratedTables.Count} 个文件到 {Config.SavedPath}");
        }
        catch (Exception ex)
        {
            await ShowExceptionAsync(ex, "保存失败");
        }
    }

    [RelayCommand]
    private async Task CopyAsync(GeneratedTableItem? item)
    {
        if (item is null || dialogService is null) return;
        await dialogService.CopyToClipboardAsync(item.GeneratedResult);
    }

    [RelayCommand]
    private async Task SelectPathAsync()
    {
        if (dialogService is null) return;
        var path = await dialogService.PickFolderAsync("选择保存路径");
        if (!string.IsNullOrEmpty(path))
        {
            Config.SavedPath = path;
        }
    }

    [RelayCommand]
    private void AddAttribute() => ExternalAttributes.Add(new AttributeItemViewModel());

    [RelayCommand]
    private void ClearAttributes() => ExternalAttributes.Clear();

    private async Task ShowMessageAsync(string title, string message)
    {
        if (dialogService is not null)
        {
            await dialogService.ShowMessageAsync(title, message);
        }
    }

    private async Task ShowExceptionAsync(Exception exception, string? title = null)
    {
        if (dialogService is not null)
        {
            await dialogService.ShowExceptionAsync(exception, title);
        }
    }
}

using Avalonia.Controls;
using DatabaseUtils.Avalonia.Services;
using DatabaseUtils.Avalonia.ViewModels;

namespace DatabaseUtils.Avalonia.Views;

/// <summary>
/// 主窗口，对应原版 MainWindow + WpfApp.razor 的壳职责。
/// 仅负责窗口生命周期（DataContext 注入、退出确认），全部交互走 ViewModel 命令。
/// </summary>
public partial class MainWindow : Window
{
    private readonly DialogService dialogService;
    private bool closeConfirmed;

    public MainWindow()
    {
        InitializeComponent();
        dialogService = new DialogService(this);
    }

    protected override void OnDataContextChanged(EventArgs e)
    {
        base.OnDataContextChanged(e);
        (DataContext as MainViewModel)?.AttachDialogService(dialogService);
    }

    /// <summary>退出前确认，对应原版 OnClosing 的 MessageBox（窗口生命周期事件，只能挂在此处）。</summary>
    protected override async void OnClosing(WindowClosingEventArgs e)
    {
        base.OnClosing(e);
        if (closeConfirmed) return;

        e.Cancel = true;
        if (await dialogService.ConfirmAsync("退出", "确定退出系统?"))
        {
            closeConfirmed = true;
            Close();
        }
    }
}

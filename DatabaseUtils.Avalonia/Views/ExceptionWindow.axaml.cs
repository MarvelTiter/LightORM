using Avalonia.Controls;
using Avalonia.Input.Platform;
using Avalonia.Interactivity;

namespace DatabaseUtils.Avalonia.Views;

/// <summary>
/// 异常详情对话框：展示异常类型、消息与完整堆栈，支持一键复制详情。
/// </summary>
public partial class ExceptionWindow : Window
{
    private readonly Exception exception;

    /// <summary>供 XAML 预编译的无参构造，请使用 <see cref="ShowAsync"/> 创建对话框。</summary>
    public ExceptionWindow() : this(new Exception("未知异常"))
    {
    }

    public ExceptionWindow(Exception exception)
    {
        InitializeComponent();
        this.exception = exception;
        ExceptionTypeText.Text = exception.GetType().FullName;
        MessageText.Text = exception.Message;
        DetailText.Text = exception.ToString();
    }

    public static async Task ShowAsync(Window owner, Exception exception, string? title = null)
    {
        var window = new ExceptionWindow(exception);
        if (!string.IsNullOrEmpty(title))
        {
            window.Title = title;
        }

        await window.ShowDialog(owner);
    }

    private async void OnCopy(object? sender, RoutedEventArgs e)
    {
        var clipboard = TopLevel.GetTopLevel(this)?.Clipboard;
        if (clipboard is not null)
        {
            await clipboard.SetTextAsync(exception.ToString());
        }
    }

    private void OnClose(object? sender, RoutedEventArgs e) => Close();
}

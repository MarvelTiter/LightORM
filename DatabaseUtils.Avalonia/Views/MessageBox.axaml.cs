using Avalonia.Controls;
using Avalonia.Interactivity;

namespace DatabaseUtils.Avalonia.Views;

/// <summary>
/// 轻量消息框，替代 System.Windows.MessageBox。
/// </summary>
public partial class MessageBox : Window
{
    private bool result;

    public MessageBox()
    {
        InitializeComponent();
    }

    /// <summary>显示消息框。showCancel 为 true 时提供确定/取消两个按钮。</summary>
    public static async Task<bool> ShowAsync(Window owner, string title, string message, bool showCancel = false)
    {
        var box = new MessageBox { Title = title };
        box.MessageText.Text = message;
        box.CancelButton.IsVisible = showCancel;
        await box.ShowDialog(owner);
        return box.result;
    }

    private void OnOk(object? sender, RoutedEventArgs e)
    {
        result = true;
        Close();
    }

    private void OnCancel(object? sender, RoutedEventArgs e) => Close();
}

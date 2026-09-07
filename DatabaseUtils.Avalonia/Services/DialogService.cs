using Avalonia.Controls;
using Avalonia.Input.Platform;
using Avalonia.Platform.Storage;
using DatabaseUtils.Avalonia.Views;

namespace DatabaseUtils.Avalonia.Services;

/// <summary>
/// 基于宿主窗口的对话框服务实现。
/// </summary>
public class DialogService(Window owner) : IDialogService
{
    public async Task ShowMessageAsync(string title, string message)
    {
        await MessageBox.ShowAsync(owner, title, message);
    }

    public async Task<bool> ConfirmAsync(string title, string message)
    {
        return await MessageBox.ShowAsync(owner, title, message, showCancel: true);
    }

    public async Task ShowExceptionAsync(Exception exception, string? title = null)
    {
        await ExceptionWindow.ShowAsync(owner, exception, title);
    }

    public async Task<string?> PickFolderAsync(string title)
    {
        var folders = await owner.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
        {
            Title = title,
            AllowMultiple = false,
        });

        return folders.Count > 0 ? folders[0].Path.LocalPath : null;
    }

    public async Task CopyToClipboardAsync(string? text)
    {
        if (string.IsNullOrEmpty(text)) return;
        var clipboard = TopLevel.GetTopLevel(owner)?.Clipboard;
        if (clipboard is not null)
        {
            await clipboard.SetTextAsync(text);
        }
    }

    public async Task ShowSettingsDialogAsync()
    {
        var window = new SettingsWindow { DataContext = owner.DataContext };
        await window.ShowDialog(owner);
    }

    public async Task ShowAttributesDialogAsync()
    {
        var window = new AttributeSettingsWindow { DataContext = owner.DataContext };
        await window.ShowDialog(owner);
    }
}

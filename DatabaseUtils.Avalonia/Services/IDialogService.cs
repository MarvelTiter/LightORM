namespace DatabaseUtils.Avalonia.Services;

/// <summary>
/// 平台服务抽象，隔离 ViewModel 对窗口、剪贴板、文件对话框等 UI 能力的依赖。
/// </summary>
public interface IDialogService
{
    /// <summary>显示消息提示框（仅确定按钮）。</summary>
    Task ShowMessageAsync(string title, string message);

    /// <summary>显示确认框（确定/取消），返回用户是否点了确定。</summary>
    Task<bool> ConfirmAsync(string title, string message);

    /// <summary>显示异常详情对话框（类型/消息/堆栈，可复制详情）。</summary>
    Task ShowExceptionAsync(Exception exception, string? title = null);

    /// <summary>选择文件夹，返回本地路径，取消时返回 null。</summary>
    Task<string?> PickFolderAsync(string title);

    /// <summary>复制文本到系统剪贴板。</summary>
    Task CopyToClipboardAsync(string? text);

    /// <summary>打开设置对话框。</summary>
    Task ShowSettingsDialogAsync();

    /// <summary>打开属性 Attribute 配置对话框。</summary>
    Task ShowAttributesDialogAsync();
}

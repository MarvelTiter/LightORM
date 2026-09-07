using CommunityToolkit.Mvvm.ComponentModel;

namespace DatabaseUtils.Avalonia.ViewModels;

/// <summary>
/// 自定义 Attribute 模板条目，{0} 占位符用于填充 Comment。
/// </summary>
public partial class AttributeItemViewModel : ObservableObject
{
    [ObservableProperty]
    public partial string? Value { get; set; }
}

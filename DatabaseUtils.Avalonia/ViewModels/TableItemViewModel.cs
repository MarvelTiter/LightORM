using CommunityToolkit.Mvvm.ComponentModel;
using DatabaseUtils.Models;

namespace DatabaseUtils.Avalonia.ViewModels;

/// <summary>
/// 数据库表条目，包装 <see cref="DatabaseTable"/> 并提供可绑定的选中状态。
/// </summary>
public partial class TableItemViewModel(DatabaseTable table) : ObservableObject
{
    public DatabaseTable Table { get; set; } = table;

    public string? TableName => Table.Table.TableName;

    [ObservableProperty]
    public partial bool IsSelected { get; set; }
}

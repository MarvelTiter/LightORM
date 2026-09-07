using Avalonia.Controls;
using Avalonia.Interactivity;

namespace DatabaseUtils.Avalonia.Views;

public partial class SettingsWindow : Window
{
    public SettingsWindow()
    {
        InitializeComponent();
    }

    private void OnClose(object? sender, RoutedEventArgs e) => Close();
}

using Avalonia.Controls;
using Avalonia.Interactivity;

namespace DatabaseUtils.Avalonia.Views;

public partial class AttributeSettingsWindow : Window
{
    public AttributeSettingsWindow()
    {
        InitializeComponent();
    }

    private void OnClose(object? sender, RoutedEventArgs e) => Close();
}

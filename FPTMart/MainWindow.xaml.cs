using System.Windows;
using FPTMart.ViewModels;
using FPTMart.Views;
using Microsoft.Extensions.DependencyInjection;

namespace FPTMart;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private readonly MainViewModel _viewModel;

    public MainWindow()
    {
        InitializeComponent();
        
        // Get MainViewModel from DI
        _viewModel = App.ServiceProvider.GetRequiredService<MainViewModel>();
        DataContext = _viewModel;
        
        // Subscribe to logout event
        _viewModel.LogoutRequested += OnLogoutRequested;
        
        // Navigate to Dashboard by default
        if (_viewModel.MenuItems.Count > 0)
        {
            _viewModel.SelectedMenuIndex = 0;
        }
    }

    private void OnLogoutRequested()
    {
        // Open login window
        var loginWindow = App.ServiceProvider.GetRequiredService<LoginWindow>();
        loginWindow.Show();
        
        // Close main window
        Close();
    }

    protected override void OnClosed(EventArgs e)
    {
        _viewModel.LogoutRequested -= OnLogoutRequested;
        base.OnClosed(e);
    }
}
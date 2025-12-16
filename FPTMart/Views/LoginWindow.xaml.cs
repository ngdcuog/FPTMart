using System.Windows;
using System.Windows.Controls;
using FPTMart.BLL.Services;
using FPTMart.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace FPTMart.Views;

public partial class LoginWindow : Window
{
    private readonly LoginViewModel _viewModel;

    public LoginWindow()
    {
        InitializeComponent();
        _viewModel = App.ServiceProvider.GetRequiredService<LoginViewModel>();
        DataContext = _viewModel;

        // Subscribe to login success event
        _viewModel.LoginSucceeded += OnLoginSucceeded;

        // Focus username textbox
        Loaded += (s, e) => UsernameTextBox.Focus();
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        Application.Current.Shutdown();
    }

    private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
    {
        if (sender is PasswordBox pb)
        {
            _viewModel.Password = pb.Password;
        }
    }

    private void OnLoginSucceeded()
    {
        // Check if password change is required
        var authService = App.ServiceProvider.GetRequiredService<IAuthService>();
        var currentUser = authService.GetCurrentUser();

        if (currentUser != null && currentUser.MustChangePassword)
        {
            // Show change password dialog
            var changePasswordDialog = new ChangePasswordDialog(currentUser.Id);
            changePasswordDialog.Owner = this;
            
            var result = changePasswordDialog.ShowDialog();
            
            if (result != true || !changePasswordDialog.PasswordChanged)
            {
                // User cancelled - logout
                authService.SetCurrentUser(null);
                MessageBox.Show("Bạn phải đổi mật khẩu để tiếp tục sử dụng hệ thống.", "Thông Báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
        }

        // Open main window
        var mainWindow = App.ServiceProvider.GetRequiredService<MainWindow>();
        mainWindow.Show();

        // Close login window
        Close();
    }

    protected override void OnClosed(EventArgs e)
    {
        _viewModel.LoginSucceeded -= OnLoginSucceeded;
        base.OnClosed(e);
    }
}

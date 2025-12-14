using System.Windows;
using FPTMart.BLL.Services;
using Microsoft.Extensions.DependencyInjection;

namespace FPTMart.Views;

public partial class ChangePasswordDialog : Window
{
    private readonly IAuthService _authService;
    private readonly int _userId;

    public bool PasswordChanged { get; private set; }

    public ChangePasswordDialog(int userId)
    {
        InitializeComponent();
        _authService = App.ServiceProvider.GetRequiredService<IAuthService>();
        _userId = userId;
    }

    private async void ChangePassword_Click(object sender, RoutedEventArgs e)
    {
        ErrorText.Text = "";

        var oldPassword = OldPasswordBox.Password;
        var newPassword = NewPasswordBox.Password;
        var confirmPassword = ConfirmPasswordBox.Password;

        // Validation
        if (string.IsNullOrWhiteSpace(oldPassword))
        {
            ErrorText.Text = "Vui lòng nhập mật khẩu cũ";
            return;
        }

        if (string.IsNullOrWhiteSpace(newPassword))
        {
            ErrorText.Text = "Vui lòng nhập mật khẩu mới";
            return;
        }

        if (newPassword.Length < 6)
        {
            ErrorText.Text = "Mật khẩu mới phải có ít nhất 6 ký tự";
            return;
        }

        if (newPassword != confirmPassword)
        {
            ErrorText.Text = "Xác nhận mật khẩu không khớp";
            return;
        }

        if (oldPassword == newPassword)
        {
            ErrorText.Text = "Mật khẩu mới phải khác mật khẩu cũ";
            return;
        }

        try
        {
            var result = await _authService.ChangePasswordAsync(_userId, oldPassword, newPassword);
            if (result)
            {
                PasswordChanged = true;
                MessageBox.Show("Đổi mật khẩu thành công!", "Thành Công", MessageBoxButton.OK, MessageBoxImage.Information);
                DialogResult = true;
                Close();
            }
            else
            {
                ErrorText.Text = "Mật khẩu cũ không đúng";
            }
        }
        catch (Exception ex)
        {
            ErrorText.Text = $"Lỗi: {ex.Message}";
        }
    }
}

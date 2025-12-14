using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FPTMart.BLL.DTOs;
using FPTMart.BLL.Services;
using System.Collections.ObjectModel;
using System.Windows;

namespace FPTMart.ViewModels;

public partial class UserManagementViewModel : BaseViewModel
{
    private readonly IUserService _userService;

    [ObservableProperty]
    private ObservableCollection<UserDto> _users = new();

    [ObservableProperty]
    private UserDto? _selectedUser;

    [ObservableProperty]
    private List<RoleDto> _roles = new();

    [ObservableProperty]
    private RoleDto? _selectedRole;

    [ObservableProperty]
    private bool _isAddDialogOpen;

    // New user form (no password - auto generated)
    [ObservableProperty]
    private string _newUsername = string.Empty;

    [ObservableProperty]
    private string _newFullName = string.Empty;

    [ObservableProperty]
    private string _newEmail = string.Empty;

    [ObservableProperty]
    private string? _newPhone;

    public UserManagementViewModel(IUserService userService)
    {
        _userService = userService;
        _ = LoadDataAsync();
    }

    private async Task LoadDataAsync()
    {
        try
        {
            IsLoading = true;

            var users = await _userService.GetAllUsersAsync();
            Users = new ObservableCollection<UserDto>(users);

            Roles = (await _userService.GetAllRolesAsync()).ToList();
        }
        catch (Exception ex)
        {
            SetError(ex.Message);
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private void AddUser()
    {
        NewUsername = string.Empty;
        NewFullName = string.Empty;
        NewEmail = string.Empty;
        NewPhone = null;
        SelectedRole = null;
        IsAddDialogOpen = true;
    }

    [RelayCommand]
    private async Task SaveUserAsync()
    {
        if (string.IsNullOrWhiteSpace(NewUsername))
        {
            MessageBox.Show("Vui lòng nhập tên đăng nhập!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (string.IsNullOrWhiteSpace(NewFullName))
        {
            MessageBox.Show("Vui lòng nhập họ tên!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (string.IsNullOrWhiteSpace(NewEmail))
        {
            MessageBox.Show("Vui lòng nhập email (để gửi mật khẩu)!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (SelectedRole == null)
        {
            MessageBox.Show("Vui lòng chọn vai trò!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        try
        {
            var newUser = await _userService.CreateUserAsync(new CreateUserDto
            {
                Username = NewUsername.Trim(),
                FullName = NewFullName.Trim(),
                Email = NewEmail.Trim(),
                Phone = NewPhone?.Trim(),
                RoleId = SelectedRole.Id
            });

            Users.Add(newUser);
            IsAddDialogOpen = false;
            MessageBox.Show($"Đã tạo tài khoản: {newUser.Username}\n\nMật khẩu tạm đã được gửi đến: {NewEmail}", "Thành Công", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Lỗi: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    [RelayCommand]
    private void CancelAdd()
    {
        IsAddDialogOpen = false;
    }

    [RelayCommand]
    private async Task ToggleStatusAsync(UserDto user)
    {
        if (user == null) return;

        try
        {
            bool success;
            if (user.IsActive)
            {
                success = await _userService.DeactivateUserAsync(user.Id);
            }
            else
            {
                success = await _userService.ActivateUserAsync(user.Id);
            }

            if (success)
            {
                await LoadDataAsync(); // Refresh
                MessageBox.Show($"Đã {(user.IsActive ? "vô hiệu hóa" : "kích hoạt")} tài khoản {user.Username}", "Thành Công", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Lỗi: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    [RelayCommand]
    private async Task ResetPasswordAsync(UserDto user)
    {
        if (user == null) return;

        if (string.IsNullOrEmpty(user.Email))
        {
            MessageBox.Show("Người dùng chưa có email!\nKhông thể gửi mật khẩu mới.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var result = MessageBox.Show(
            $"Đặt lại mật khẩu cho '{user.FullName}'?\n\nMật khẩu mới sẽ được gửi đến: {user.Email}",
            "Xác Nhận",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);

        if (result == MessageBoxResult.Yes)
        {
            try
            {
                await _userService.ResetPasswordAsync(user.Id, "");
                MessageBox.Show($"Đã gửi mật khẩu mới đến {user.Email}", "Thành Công", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}

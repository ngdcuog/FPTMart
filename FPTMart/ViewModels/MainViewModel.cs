using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FPTMart.BLL.DTOs;
using FPTMart.BLL.Services;
using System.Collections.ObjectModel;

namespace FPTMart.ViewModels;

public partial class MainViewModel : BaseViewModel
{
    private readonly IAuthService _authService;
    
    [ObservableProperty]
    private object? _currentPage;

    [ObservableProperty]
    private string _currentPageTitle = "Dashboard";

    [ObservableProperty]
    private bool _isLoggedIn;

    [ObservableProperty]
    private UserDto? _currentUser;

    [ObservableProperty]
    private int _selectedMenuIndex;

    [ObservableProperty]
    private ObservableCollection<MenuItem> _menuItems = new();

    public MainViewModel(IAuthService authService)
    {
        _authService = authService;
        CheckLoginStatus();
        BuildMenuBasedOnRole();
    }

    private void CheckLoginStatus()
    {
        var user = _authService.GetCurrentUser();
        if (user != null)
        {
            IsLoggedIn = true;
            CurrentUser = user;
        }
    }

    private void BuildMenuBasedOnRole()
    {
        MenuItems.Clear();

        // Dashboard - everyone
        MenuItems.Add(new MenuItem { Title = "Dashboard", Icon = "ViewDashboard", PageType = typeof(Views.DashboardView) });

        var user = _authService.GetCurrentUser();
        if (user == null) return;

        // Admin menu
        if (user.Roles.Contains("Admin"))
        {
            MenuItems.Add(new MenuItem { Title = "Quản Lý User", Icon = "AccountCog", PageType = typeof(Views.UserManagementView) });
        }

        // Manager menu (full management)
        if (user.Roles.Contains("Admin") || user.Roles.Contains("Manager"))
        {
            MenuItems.Add(new MenuItem { Title = "Sản Phẩm", Icon = "Package", PageType = typeof(Views.ProductListView) });
            MenuItems.Add(new MenuItem { Title = "Danh Mục", Icon = "Shape", PageType = typeof(Views.CategoryManagementView) });
            MenuItems.Add(new MenuItem { Title = "Nhập Kho", Icon = "TruckDelivery", PageType = typeof(Views.StockInView) });
            MenuItems.Add(new MenuItem { Title = "Nhà Cung Cấp", Icon = "Factory", PageType = typeof(Views.SupplierManagementView) });
            MenuItems.Add(new MenuItem { Title = "Khách Hàng", Icon = "AccountGroup", PageType = typeof(Views.CustomerListView) });
            MenuItems.Add(new MenuItem { Title = "Báo Cáo", Icon = "ChartBar", PageType = typeof(Views.ReportView) });
        }
        
        // StockKeeper menu (stock management only - NO sales)
        if (user.Roles.Contains("StockKeeper") && !user.Roles.Contains("Admin") && !user.Roles.Contains("Manager"))
        {
            MenuItems.Add(new MenuItem { Title = "Sản Phẩm", Icon = "Package", PageType = typeof(Views.ProductListView) });
            MenuItems.Add(new MenuItem { Title = "Nhập Kho", Icon = "TruckDelivery", PageType = typeof(Views.StockInView) });
        }

        // Cashier/POS menu - NOT for StockKeeper only
        if (!user.Roles.Contains("StockKeeper") || user.Roles.Contains("Admin") || user.Roles.Contains("Manager") || user.Roles.Contains("Cashier"))
        {
            MenuItems.Add(new MenuItem { Title = "Bán Hàng", Icon = "CashRegister", PageType = typeof(Views.POSView) });
        }
    }

    [RelayCommand]
    private void NavigateTo(MenuItem item)
    {
        if (item?.PageType == null) return;
        
        CurrentPageTitle = item.Title;
        
        // Create instance of the page with DI
        var page = App.ServiceProvider.GetService(item.PageType);
        if (page == null)
        {
            page = Activator.CreateInstance(item.PageType);
        }
        CurrentPage = page;
    }

    // Event for logout
    public event Action? LogoutRequested;

    [RelayCommand]
    private void Logout()
    {
        _authService.SetCurrentUser(null);
        IsLoggedIn = false;
        CurrentUser = null;
        LogoutRequested?.Invoke();
    }

    partial void OnSelectedMenuIndexChanged(int value)
    {
        if (value >= 0 && value < MenuItems.Count)
        {
            NavigateTo(MenuItems[value]);
        }
    }
}

public class MenuItem
{
    public string Title { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
    public Type? PageType { get; set; }
    public List<string> RequiredRoles { get; set; } = new();
}

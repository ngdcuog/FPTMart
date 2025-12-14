using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FPTMart.BLL.DTOs;
using FPTMart.BLL.Services;
using System.Collections.ObjectModel;
using System.Windows;

namespace FPTMart.ViewModels;

public partial class CustomerListViewModel : BaseViewModel
{
    private readonly ICustomerService _customerService;

    [ObservableProperty]
    private ObservableCollection<CustomerDto> _customers = new();

    [ObservableProperty]
    private CustomerDto? _selectedCustomer;

    [ObservableProperty]
    private string _searchText = string.Empty;

    // For add dialog
    [ObservableProperty]
    private string _newCustomerName = string.Empty;

    [ObservableProperty]
    private string _newCustomerPhone = string.Empty;

    [ObservableProperty]
    private bool _isAddDialogOpen;

    public CustomerListViewModel(ICustomerService customerService)
    {
        _customerService = customerService;
        _ = LoadDataAsync();
    }

    private async Task LoadDataAsync()
    {
        try
        {
            IsLoading = true;
            var customers = await _customerService.GetAllCustomersAsync();
            Customers = new ObservableCollection<CustomerDto>(customers);
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
    private async Task SearchAsync()
    {
        if (string.IsNullOrWhiteSpace(SearchText))
        {
            await LoadDataAsync();
            return;
        }

        try
        {
            var results = await _customerService.SearchCustomersAsync(SearchText);
            Customers = new ObservableCollection<CustomerDto>(results);
        }
        catch (Exception ex)
        {
            SetError(ex.Message);
        }
    }

    [RelayCommand]
    private void AddCustomer()
    {
        NewCustomerName = string.Empty;
        NewCustomerPhone = string.Empty;
        IsAddDialogOpen = true;
    }

    [RelayCommand]
    private async Task SaveNewCustomerAsync()
    {
        if (string.IsNullOrWhiteSpace(NewCustomerName))
        {
            MessageBox.Show("Vui lòng nhập tên khách hàng!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        try
        {
            var customer = await _customerService.CreateCustomerAsync(new CustomerDto
            {
                FullName = NewCustomerName.Trim(),
                Phone = string.IsNullOrWhiteSpace(NewCustomerPhone) ? null : NewCustomerPhone.Trim()
            });

            Customers.Add(customer);
            IsAddDialogOpen = false;
            MessageBox.Show($"Đã thêm khách hàng: {customer.FullName}", "Thành Công", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Lỗi: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    [RelayCommand]
    private void CancelAddCustomer()
    {
        IsAddDialogOpen = false;
    }

    [RelayCommand]
    private void EditCustomer(CustomerDto customer)
    {
        if (customer == null) return;
        MessageBox.Show($"Sửa khách hàng: {customer.FullName}\n(Chức năng đang phát triển)", "Thông Báo", MessageBoxButton.OK, MessageBoxImage.Information);
    }
}

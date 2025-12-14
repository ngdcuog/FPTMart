using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FPTMart.BLL.DTOs;
using FPTMart.BLL.Services;
using System.Collections.ObjectModel;
using System.Windows;

namespace FPTMart.ViewModels;

public partial class SupplierManagementViewModel : BaseViewModel
{
    private readonly ISupplierService _supplierService;

    [ObservableProperty]
    private ObservableCollection<SupplierDto> _suppliers = new();

    [ObservableProperty]
    private string _searchText = string.Empty;

    [ObservableProperty]
    private bool _isDialogOpen;

    [ObservableProperty]
    private string _dialogTitle = "Thêm Nhà Cung Cấp";

    [ObservableProperty]
    private SupplierDto _editingSupplier = new();

    private bool _isEditing;

    public SupplierManagementViewModel(ISupplierService supplierService)
    {
        _supplierService = supplierService;
        _ = LoadSuppliersAsync();
    }

    private async Task LoadSuppliersAsync()
    {
        try
        {
            IsLoading = true;
            var suppliers = await _supplierService.GetAllSuppliersAsync();
            
            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                suppliers = suppliers.Where(s => 
                    s.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                    (s.Phone?.Contains(SearchText) ?? false));
            }
            
            Suppliers = new ObservableCollection<SupplierDto>(suppliers);
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

    partial void OnSearchTextChanged(string value)
    {
        _ = LoadSuppliersAsync();
    }

    [RelayCommand]
    private void AddSupplier()
    {
        EditingSupplier = new SupplierDto();
        DialogTitle = "Thêm Nhà Cung Cấp";
        _isEditing = false;
        IsDialogOpen = true;
    }

    [RelayCommand]
    private void EditSupplier(SupplierDto supplier)
    {
        if (supplier == null) return;
        
        EditingSupplier = new SupplierDto
        {
            Id = supplier.Id,
            Name = supplier.Name,
            ContactPerson = supplier.ContactPerson,
            Phone = supplier.Phone
        };
        DialogTitle = "Sửa Nhà Cung Cấp";
        _isEditing = true;
        IsDialogOpen = true;
    }

    [RelayCommand]
    private async Task DeleteSupplierAsync(SupplierDto supplier)
    {
        if (supplier == null) return;

        var result = MessageBox.Show(
            $"Bạn có chắc muốn xóa nhà cung cấp '{supplier.Name}'?",
            "Xác Nhận",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);

        if (result == MessageBoxResult.Yes)
        {
            // Soft delete by setting IsActive = false
            try
            {
                supplier.IsActive = false;
                await _supplierService.UpdateSupplierAsync(supplier);
                await LoadSuppliersAsync();
                MessageBox.Show("Đã xóa nhà cung cấp!", "Thông Báo", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    [RelayCommand]
    private async Task SaveSupplierAsync()
    {
        if (string.IsNullOrWhiteSpace(EditingSupplier.Name))
        {
            MessageBox.Show("Vui lòng nhập tên nhà cung cấp!", "Thông Báo", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        try
        {
            if (_isEditing)
            {
                await _supplierService.UpdateSupplierAsync(EditingSupplier);
            }
            else
            {
                await _supplierService.CreateSupplierAsync(EditingSupplier);
            }

            IsDialogOpen = false;
            await LoadSuppliersAsync();
            MessageBox.Show("Lưu thành công!", "Thông Báo", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Lỗi: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    [RelayCommand]
    private void CancelDialog()
    {
        IsDialogOpen = false;
    }
}

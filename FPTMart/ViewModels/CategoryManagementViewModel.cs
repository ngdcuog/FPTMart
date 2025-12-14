using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FPTMart.BLL.DTOs;
using FPTMart.BLL.Services;
using System.Collections.ObjectModel;
using System.Windows;

namespace FPTMart.ViewModels;

public partial class CategoryManagementViewModel : BaseViewModel
{
    private readonly ICategoryService _categoryService;

    [ObservableProperty]
    private ObservableCollection<CategoryDto> _categories = new();

    [ObservableProperty]
    private string _searchText = string.Empty;

    [ObservableProperty]
    private bool _isDialogOpen;

    [ObservableProperty]
    private string _dialogTitle = "Thêm Danh Mục";

    [ObservableProperty]
    private CategoryDto _editingCategory = new();

    private bool _isEditing;

    public CategoryManagementViewModel(ICategoryService categoryService)
    {
        _categoryService = categoryService;
        _ = LoadCategoriesAsync();
    }

    private async Task LoadCategoriesAsync()
    {
        try
        {
            IsLoading = true;
            var categories = await _categoryService.GetAllCategoriesAsync();
            
            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                categories = categories.Where(c => 
                    c.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase));
            }
            
            Categories = new ObservableCollection<CategoryDto>(categories);
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
        _ = LoadCategoriesAsync();
    }

    [RelayCommand]
    private void AddCategory()
    {
        EditingCategory = new CategoryDto();
        DialogTitle = "Thêm Danh Mục";
        _isEditing = false;
        IsDialogOpen = true;
    }

    [RelayCommand]
    private void EditCategory(CategoryDto category)
    {
        if (category == null) return;
        
        EditingCategory = new CategoryDto
        {
            Id = category.Id,
            Name = category.Name,
            Description = category.Description,
            IsActive = category.IsActive
        };
        DialogTitle = "Sửa Danh Mục";
        _isEditing = true;
        IsDialogOpen = true;
    }

    [RelayCommand]
    private async Task DeleteCategoryAsync(CategoryDto category)
    {
        if (category == null) return;

        if (category.ProductCount > 0)
        {
            MessageBox.Show(
                $"Không thể xóa danh mục '{category.Name}' vì đang có {category.ProductCount} sản phẩm!",
                "Thông Báo",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
            return;
        }

        var result = MessageBox.Show(
            $"Bạn có chắc muốn xóa danh mục '{category.Name}'?",
            "Xác Nhận",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);

        if (result == MessageBoxResult.Yes)
        {
            try
            {
                await _categoryService.DeleteCategoryAsync(category.Id);
                await LoadCategoriesAsync();
                MessageBox.Show("Đã xóa danh mục!", "Thông Báo", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    [RelayCommand]
    private async Task SaveCategoryAsync()
    {
        if (string.IsNullOrWhiteSpace(EditingCategory.Name))
        {
            MessageBox.Show("Vui lòng nhập tên danh mục!", "Thông Báo", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        try
        {
            if (_isEditing)
            {
                await _categoryService.UpdateCategoryAsync(EditingCategory);
            }
            else
            {
                await _categoryService.CreateCategoryAsync(EditingCategory);
            }

            IsDialogOpen = false;
            await LoadCategoriesAsync();
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

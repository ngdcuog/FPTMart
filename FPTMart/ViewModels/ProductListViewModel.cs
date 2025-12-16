using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FPTMart.BLL.DTOs;
using FPTMart.BLL.Services;
using FPTMart.Views;
using System.Collections.ObjectModel;
using System.Windows;

namespace FPTMart.ViewModels;

public partial class ProductListViewModel : BaseViewModel
{
    private readonly IProductService _productService;
    private readonly ICategoryService _categoryService;

    [ObservableProperty]
    private ObservableCollection<ProductDto> _products = new();

    [ObservableProperty]
    private ProductDto? _selectedProduct;

    [ObservableProperty]
    private string _searchText = string.Empty;

    [ObservableProperty]
    private List<CategoryDto> _categories = new();

    public ProductListViewModel(IProductService productService, ICategoryService categoryService)
    {
        _productService = productService;
        _categoryService = categoryService;
        _ = LoadDataAsync();
    }

    private async Task LoadDataAsync()
    {
        try
        {
            IsLoading = true;
            var products = await _productService.GetAllProductsAsync();
            Products = new ObservableCollection<ProductDto>(products.Where(p => p.IsActive));
            
            Categories = (await _categoryService.GetActiveCategoriesAsync()).ToList();
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
        try
        {
            IsLoading = true;
            if (string.IsNullOrWhiteSpace(SearchText))
            {
                await LoadDataAsync();
            }
            else
            {
                var products = await _productService.SearchProductsAsync(SearchText);
                Products = new ObservableCollection<ProductDto>(products);
            }
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task RefreshAsync()
    {
        SearchText = string.Empty;
        await LoadDataAsync();
    }

    [RelayCommand]
    private async Task AddProductAsync()
    {
        var dialog = new ProductDialog(null);
        var mainWin = Application.Current.MainWindow;
        if (mainWin != null && mainWin != dialog) dialog.Owner = mainWin;
        
        if (dialog.ShowDialog() == true && dialog.Result != null)
        {
            Products.Add(dialog.Result);
        }
    }

    [RelayCommand]
    private async Task EditProductAsync(ProductDto product)
    {
        if (product == null) return;

        var dialog = new ProductDialog(product);
        var mainWin = Application.Current.MainWindow;
        if (mainWin != null && mainWin != dialog) dialog.Owner = mainWin;
        
        if (dialog.ShowDialog() == true && dialog.Result != null)
        {
            // Update in list
            var index = Products.IndexOf(product);
            if (index >= 0)
            {
                Products[index] = dialog.Result;
            }
        }
    }

    [RelayCommand]
    private async Task DeleteProductAsync(ProductDto product)
    {
        if (product == null) return;
        
        var result = MessageBox.Show(
            $"Bạn có chắc muốn xóa sản phẩm '{product.Name}'?",
            "Xác Nhận Xóa",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);

        if (result == MessageBoxResult.Yes)
        {
            try
            {
                await _productService.DeleteProductAsync(product.Id);
                Products.Remove(product);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}

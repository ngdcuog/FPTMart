using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FPTMart.BLL.DTOs;
using FPTMart.BLL.Services;

namespace FPTMart.ViewModels;

public partial class ProductDialogViewModel : BaseViewModel
{
    private readonly IProductService _productService;
    private readonly ICategoryService _categoryService;

    public event Action<bool>? RequestClose;

    [ObservableProperty]
    private ProductDto _product = new();

    [ObservableProperty]
    private List<CategoryDto> _categories = new();

    [ObservableProperty]
    private string _dialogTitle = "Thêm Sản Phẩm";

    [ObservableProperty]
    private bool _isNew = true;

    public ProductDialogViewModel(IProductService productService, ICategoryService categoryService)
    {
        _productService = productService;
        _categoryService = categoryService;
    }

    public async void Initialize(ProductDto? existingProduct)
    {
        // Load categories
        Categories = (await _categoryService.GetActiveCategoriesAsync()).ToList();

        if (existingProduct != null)
        {
            // Edit mode
            Product = new ProductDto
            {
                Id = existingProduct.Id,
                ProductCode = existingProduct.ProductCode,
                Barcode = existingProduct.Barcode,
                Name = existingProduct.Name,
                Description = existingProduct.Description,
                CategoryId = existingProduct.CategoryId,
                CostPrice = existingProduct.CostPrice,
                SellingPrice = existingProduct.SellingPrice,
                StockQuantity = existingProduct.StockQuantity,
                MinStockLevel = existingProduct.MinStockLevel,
                UnitsPerCase = existingProduct.UnitsPerCase,
                CaseUnit = existingProduct.CaseUnit,
                Unit = existingProduct.Unit,
                ImagePath = existingProduct.ImagePath,
                IsActive = existingProduct.IsActive
            };
            DialogTitle = "Sửa Sản Phẩm";
            IsNew = false;
        }
        else
        {
            // New product
            Product = new ProductDto
            {
                ProductCode = await _productService.GenerateProductCodeAsync(),
                IsActive = true,
                MinStockLevel = 10,
                Unit = "Cái"
            };
            DialogTitle = "Thêm Sản Phẩm";
            IsNew = true;
        }
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        // Validation
        if (string.IsNullOrWhiteSpace(Product.ProductCode))
        {
            ErrorMessage = "Vui lòng nhập mã sản phẩm";
            return;
        }

        if (string.IsNullOrWhiteSpace(Product.Name))
        {
            ErrorMessage = "Vui lòng nhập tên sản phẩm";
            return;
        }

        if (Product.CategoryId <= 0)
        {
            ErrorMessage = "Vui lòng chọn danh mục";
            return;
        }

        if (Product.SellingPrice <= 0)
        {
            ErrorMessage = "Giá bán phải lớn hơn 0";
            return;
        }

        if (Product.CostPrice >= Product.SellingPrice)
        {
            ErrorMessage = "Giá nhập phải nhỏ hơn giá bán!";
            return;
        }

        try
        {
            IsLoading = true;

            if (IsNew)
            {
                Product = await _productService.CreateProductAsync(Product);
            }
            else
            {
                Product = await _productService.UpdateProductAsync(Product);
            }

            RequestClose?.Invoke(true);
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Lỗi: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private void Cancel()
    {
        RequestClose?.Invoke(false);
    }
}

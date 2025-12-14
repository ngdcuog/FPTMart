using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FPTMart.BLL.DTOs;
using FPTMart.BLL.Services;
using System.Collections.ObjectModel;
using System.Windows;

namespace FPTMart.ViewModels;

public partial class StockInViewModel : BaseViewModel
{
    private readonly IProductService _productService;
    private readonly ISupplierService _supplierService;
    private readonly IStockService _stockService;

    [ObservableProperty]
    private ObservableCollection<ProductDto> _products = new();

    [ObservableProperty]
    private ObservableCollection<StockInItemDto> _stockInItems = new();

    [ObservableProperty]
    private List<SupplierDto> _suppliers = new();

    [ObservableProperty]
    private SupplierDto? _selectedSupplier;

    [ObservableProperty]
    private string _searchText = string.Empty;

    [ObservableProperty]
    private decimal _totalAmount;

    // History
    [ObservableProperty]
    private ObservableCollection<StockInDto> _stockInHistory = new();

    [ObservableProperty]
    private int _selectedTabIndex = 0;

    public StockInViewModel(IProductService productService, ISupplierService supplierService, IStockService stockService)
    {
        _productService = productService;
        _supplierService = supplierService;
        _stockService = stockService;

        _ = LoadDataAsync();
    }

    private async Task LoadDataAsync()
    {
        try
        {
            IsLoading = true;

            var products = await _productService.GetAllProductsAsync();
            Products = new ObservableCollection<ProductDto>(products.Where(p => p.IsActive));

            Suppliers = (await _supplierService.GetActiveSuppliersAsync()).ToList();
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
    private async Task LoadHistoryAsync()
    {
        try
        {
            IsLoading = true;
            var history = await _stockService.GetAllStockInsAsync();
            StockInHistory = new ObservableCollection<StockInDto>(history);
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

        var results = await _productService.SearchProductsAsync(SearchText);
        Products = new ObservableCollection<ProductDto>(results);
    }

    [RelayCommand]
    private void AddToStockIn(ProductDto product)
    {
        if (product == null) return;

        var existingItem = StockInItems.FirstOrDefault(i => i.ProductId == product.Id);
        if (existingItem != null)
        {
            existingItem.CaseQuantity++;
            CalculateTotal();
            // Force UI refresh
            var index = StockInItems.IndexOf(existingItem);
            StockInItems[index] = existingItem;
            return;
        }

        StockInItems.Add(new StockInItemDto
        {
            ProductId = product.Id,
            ProductCode = product.ProductCode,
            ProductName = product.Name,
            UnitsPerCase = product.UnitsPerCase,
            CaseUnit = product.CaseUnit,
            Unit = product.Unit,
            CaseQuantity = 1,
            CostPrice = product.CostPrice * product.UnitsPerCase // Default: cost per case
        });

        CalculateTotal();
    }

    [RelayCommand]
    private void RemoveItem(StockInItemDto item)
    {
        if (item == null) return;
        StockInItems.Remove(item);
        CalculateTotal();
    }

    private void CalculateTotal()
    {
        TotalAmount = StockInItems.Sum(i => i.TotalPrice);
    }

    [RelayCommand]
    private async Task SaveStockInAsync()
    {
        if (!StockInItems.Any())
        {
            MessageBox.Show("Vui lòng thêm sản phẩm vào phiếu nhập!", "Thông Báo", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        try
        {
            IsLoading = true;

            var stockIn = new StockInDto
            {
                SupplierId = SelectedSupplier?.Id,
                TotalAmount = TotalAmount,
                Items = StockInItems.ToList()
            };

            var result = await _stockService.CreateStockInAsync(stockIn);

            MessageBox.Show(
                $"Nhập kho thành công!\n\nMã phiếu: {result.StockInNumber}\nTổng tiền: {result.TotalAmount:N0} đ",
                "Hoàn Tất",
                MessageBoxButton.OK,
                MessageBoxImage.Information);

            // Reset
            StockInItems.Clear();
            TotalAmount = 0;
            SelectedSupplier = null;

            // Reload products to update stock
            await LoadDataAsync();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Lỗi: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            IsLoading = false;
        }
    }
}

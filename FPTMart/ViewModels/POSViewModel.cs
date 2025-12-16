using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FPTMart.BLL.DTOs;
using FPTMart.BLL.Services;
using FPTMart.Views;
using System.Collections.ObjectModel;
using System.Windows;

namespace FPTMart.ViewModels;

public partial class POSViewModel : BaseViewModel
{
    private readonly IProductService _productService;
    private readonly ISaleService _saleService;
    private readonly ICustomerService _customerService;

    [ObservableProperty]
    private ObservableCollection<ProductDto> _products = new();

    [ObservableProperty]
    private ObservableCollection<SaleItemDto> _cartItems = new();

    [ObservableProperty]
    private string _searchText = string.Empty;

    [ObservableProperty]
    private decimal _subTotal;

    [ObservableProperty]
    private decimal _discountAmount;

    [ObservableProperty]
    private decimal _totalAmount;

    [ObservableProperty]
    private CustomerDto? _selectedCustomer;

    [ObservableProperty]
    private ObservableCollection<CustomerDto> _customers = new();

    [ObservableProperty]
    private string _customerPhone = string.Empty;

    public POSViewModel(IProductService productService, ISaleService saleService, ICustomerService customerService)
    {
        _productService = productService;
        _saleService = saleService;
        _customerService = customerService;

        _ = LoadDataAsync();
    }

    private async Task LoadDataAsync()
    {
        await LoadProductsAsync();
    }

    [RelayCommand]
    private async Task SearchCustomerAsync()
    {
        if (string.IsNullOrWhiteSpace(CustomerPhone))
        {
            SelectedCustomer = null;
            return;
        }

        try
        {
            var customer = await _customerService.GetCustomerByPhoneAsync(CustomerPhone.Trim());
            if (customer != null)
            {
                SelectedCustomer = customer;
            }
            else
            {
                System.Windows.MessageBox.Show($"Không tìm thấy khách hàng với SĐT: {CustomerPhone}", "Thông Báo", 
                    System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
                SelectedCustomer = null;
            }
        }
        catch (Exception ex)
        {
            SetError(ex.Message);
        }
    }

    private async Task LoadProductsAsync()
    {
        try
        {
            var products = await _productService.GetAllProductsAsync();
            Products = new ObservableCollection<ProductDto>(products.Where(p => p.IsActive && p.StockQuantity > 0));
        }
        catch (Exception ex)
        {
            SetError(ex.Message);
        }
    }

    [RelayCommand]
    private async Task SearchProductAsync()
    {
        if (string.IsNullOrWhiteSpace(SearchText))
        {
            await LoadProductsAsync();
            return;
        }

        try
        {
            // First try barcode
            var byBarcode = await _productService.GetProductsByBarcodeAsync(SearchText);
            if (byBarcode.Any())
            {
                if (byBarcode.Count() == 1)
                {
                    AddToCart(byBarcode.First());
                    SearchText = string.Empty;
                    return;
                }
                else
                {
                    Products = new ObservableCollection<ProductDto>(byBarcode);
                    return;
                }
            }

            // Try product code
            var byCode = await _productService.GetProductByCodeAsync(SearchText);
            if (byCode != null)
            {
                AddToCart(byCode);
                SearchText = string.Empty;
                return;
            }

            // General search
            var results = await _productService.SearchProductsAsync(SearchText);
            Products = new ObservableCollection<ProductDto>(results);
        }
        catch (Exception ex)
        {
            SetError(ex.Message);
        }
    }

    [RelayCommand]
    private async Task ScanBarcodeAsync()
    {
        try
        {
            var scannerDialog = new BarcodeScannerDialog();
            var mainWin = Application.Current.MainWindow;
            if (mainWin != null && mainWin != scannerDialog) scannerDialog.Owner = mainWin;

            if (scannerDialog.ShowDialog() == true && !string.IsNullOrEmpty(scannerDialog.ScannedBarcode))
            {
                var barcode = scannerDialog.ScannedBarcode;
                
                // Try to find product by barcode
                var byBarcode = await _productService.GetProductsByBarcodeAsync(barcode);
                if (byBarcode.Any())
                {
                    if (byBarcode.Count() == 1)
                    {
                        var product = byBarcode.First();
                        if (product.IsActive && product.StockQuantity > 0)
                        {
                            AddToCart(product);
                            // Silent add - no message
                        }
                        else
                        {
                            MessageBox.Show($"Sản phẩm '{product.Name}' hết hàng hoặc không hoạt động!", 
                                "Thông Báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                        }
                        return;
                    }
                    else
                    {
                        // Multiple products with same barcode - show in list and set search text
                        SearchText = barcode;
                        Products = new ObservableCollection<ProductDto>(byBarcode.Where(p => p.IsActive && p.StockQuantity > 0));
                        return;
                    }
                }

                // Try product code
                var byCode = await _productService.GetProductByCodeAsync(barcode);
                if (byCode != null && byCode.IsActive && byCode.StockQuantity > 0)
                {
                    AddToCart(byCode);
                    // Silent add - no message
                    return;
                }

                MessageBox.Show($"Không tìm thấy sản phẩm với mã: {barcode}", 
                    "Thông Báo", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Lỗi quét mã vạch: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    [RelayCommand]
    private void ClearCustomer()
    {
        SelectedCustomer = null;
    }

    [RelayCommand]
    private void AddToCart(ProductDto product)
    {
        if (product == null) return;

        var existingItem = CartItems.FirstOrDefault(i => i.ProductId == product.Id);
        if (existingItem != null)
        {
            // Check stock before increasing
            if (existingItem.Quantity >= product.StockQuantity)
            {
                MessageBox.Show($"Không đủ hàng!\nHiện tại: {existingItem.Quantity}\nTồn kho: {product.StockQuantity}", 
                    "Thông Báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            existingItem.Quantity++;
            existingItem.TotalPrice = existingItem.Quantity * existingItem.UnitPrice;
            var index = CartItems.IndexOf(existingItem);
            CartItems[index] = existingItem;
        }
        else
        {
            // Check stock before adding
            if (product.StockQuantity <= 0)
            {
                MessageBox.Show($"Sản phẩm '{product.Name}' đã hết hàng!", 
                    "Thông Báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            
            CartItems.Add(new SaleItemDto
            {
                ProductId = product.Id,
                ProductCode = product.ProductCode,
                ProductName = product.Name,
                Quantity = 1,
                UnitPrice = product.SellingPrice,
                TotalPrice = product.SellingPrice,
                MaxQuantity = product.StockQuantity // Store for validation
            });
        }

        CalculateTotals();
    }

    [RelayCommand]
    private void RemoveFromCart(SaleItemDto item)
    {
        if (item == null) return;
        CartItems.Remove(item);
        CalculateTotals();
    }

    [RelayCommand]
    private void IncreaseQuantity(SaleItemDto item)
    {
        if (item == null) return;
        
        // Check stock limit
        if (item.Quantity >= item.MaxQuantity)
        {
            MessageBox.Show($"Không đủ hàng!\nHiện tại: {item.Quantity}\nTồn kho: {item.MaxQuantity}", 
                "Thông Báo", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }
        
        item.Quantity++;
        item.TotalPrice = item.Quantity * item.UnitPrice;
        var index = CartItems.IndexOf(item);
        CartItems[index] = item;
        CalculateTotals();
    }

    [RelayCommand]
    private void DecreaseQuantity(SaleItemDto item)
    {
        if (item == null) return;
        if (item.Quantity > 1)
        {
            item.Quantity--;
            item.TotalPrice = item.Quantity * item.UnitPrice;
            var index = CartItems.IndexOf(item);
            CartItems[index] = item;
        }
        else
        {
            CartItems.Remove(item);
        }
        CalculateTotals();
    }

    private void CalculateTotals()
    {
        SubTotal = CartItems.Sum(i => i.TotalPrice);
        TotalAmount = SubTotal - DiscountAmount;
    }

    [RelayCommand]
    private async Task CheckoutAsync()
    {
        if (!CartItems.Any())
        {
            MessageBox.Show("Vui lòng thêm sản phẩm vào giỏ hàng!", "Thông Báo", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        // Show payment dialog
        var paymentDialog = new PaymentDialog(TotalAmount);
        var mainWindow = Application.Current.MainWindow;
        if (mainWindow != null && mainWindow != paymentDialog)
        {
            paymentDialog.Owner = mainWindow;
        }

        if (paymentDialog.ShowDialog() != true)
        {
            return; // User cancelled
        }

        try
        {
            var sale = new SaleDto
            {
                CustomerId = SelectedCustomer?.Id,
                SubTotal = SubTotal,
                DiscountAmount = DiscountAmount,
                TotalAmount = TotalAmount,
                PaidAmount = paymentDialog.PaidAmount,
                ChangeAmount = paymentDialog.ChangeAmount,
                PaymentMethod = paymentDialog.PaymentMethod,
                Items = CartItems.ToList()
            };

            var result = await _saleService.CreateSaleAsync(sale);

            // Show Invoice Dialog
            var invoiceDialog = new InvoiceDialog(result);
            var mainWnd = Application.Current.MainWindow;
            if (mainWnd != null && mainWnd != invoiceDialog) invoiceDialog.Owner = mainWnd;
            invoiceDialog.ShowDialog();

            // Reset cart
            CartItems.Clear();
            SubTotal = 0;
            DiscountAmount = 0;
            TotalAmount = 0;
            SelectedCustomer = null;
            CustomerPhone = string.Empty;

            // Reload products
            await LoadProductsAsync();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Lỗi: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}

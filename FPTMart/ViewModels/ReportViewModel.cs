using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FPTMart.BLL.DTOs;
using FPTMart.BLL.Services;
using System.Collections.ObjectModel;

namespace FPTMart.ViewModels;

public partial class ReportViewModel : BaseViewModel
{
    private readonly ISaleService _saleService;
    private readonly IProductService _productService;

    [ObservableProperty]
    private DateTime _startDate = DateTime.Today.AddDays(-30);

    [ObservableProperty]
    private DateTime _endDate = DateTime.Today;

    [ObservableProperty]
    private decimal _totalRevenue;

    [ObservableProperty]
    private int _totalOrders;

    [ObservableProperty]
    private decimal _averageOrderValue;

    [ObservableProperty]
    private int _totalProductsSold;

    [ObservableProperty]
    private ObservableCollection<SaleDto> _sales = new();

    [ObservableProperty]
    private ObservableCollection<TopProductDto> _topProducts = new();

    public ReportViewModel(ISaleService saleService, IProductService productService)
    {
        _saleService = saleService;
        _productService = productService;

        _ = LoadReportAsync();
    }

    [RelayCommand]
    private async Task LoadReportAsync()
    {
        try
        {
            IsLoading = true;

            // Load sales in date range
            var endDatePlusOne = EndDate.AddDays(1);
            var sales = await _saleService.GetSalesByDateRangeAsync(StartDate, endDatePlusOne);
            Sales = new ObservableCollection<SaleDto>(sales);

            // Calculate summary
            var completedSales = sales.Where(s => s.Status == "Completed").ToList();
            TotalRevenue = completedSales.Sum(s => s.TotalAmount);
            TotalOrders = completedSales.Count;
            AverageOrderValue = TotalOrders > 0 ? TotalRevenue / TotalOrders : 0;

            // Calculate products sold
            TotalProductsSold = completedSales.Sum(s => s.Items.Sum(i => i.Quantity));

            // Calculate top products
            var productSales = new Dictionary<int, (string Code, string Name, int Qty, decimal Rev)>();
            foreach (var sale in completedSales)
            {
                foreach (var item in sale.Items)
                {
                    if (productSales.ContainsKey(item.ProductId))
                    {
                        var current = productSales[item.ProductId];
                        productSales[item.ProductId] = (current.Code, current.Name, current.Qty + item.Quantity, current.Rev + item.TotalPrice);
                    }
                    else
                    {
                        productSales[item.ProductId] = (item.ProductCode, item.ProductName, item.Quantity, item.TotalPrice);
                    }
                }
            }

            var topProductList = productSales
                .OrderByDescending(p => p.Value.Qty)
                .Take(10)
                .Select((p, index) => new TopProductDto
                {
                    Rank = index + 1,
                    ProductCode = p.Value.Code,
                    Name = p.Value.Name,
                    QuantitySold = p.Value.Qty,
                    Revenue = p.Value.Rev
                })
                .ToList();

            TopProducts = new ObservableCollection<TopProductDto>(topProductList);
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
}

public class TopProductDto
{
    public int Rank { get; set; }
    public string ProductCode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int QuantitySold { get; set; }
    public decimal Revenue { get; set; }
}

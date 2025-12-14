using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FPTMart.BLL.DTOs;
using FPTMart.BLL.Services;
using LiveCharts;
using LiveCharts.Wpf;
using System.Windows.Media;

namespace FPTMart.ViewModels;

public partial class DashboardViewModel : BaseViewModel
{
    private readonly IDashboardService _dashboardService;
    private readonly ISaleService _saleService;
    private readonly IProductService _productService;

    [ObservableProperty]
    private decimal _todayRevenue;

    [ObservableProperty]
    private int _todaySalesCount;

    [ObservableProperty]
    private int _totalProducts;

    [ObservableProperty]
    private int _lowStockCount;

    [ObservableProperty]
    private List<SaleDto> _recentSales = new();

    [ObservableProperty]
    private List<ProductDto> _lowStockProducts = new();

    // LiveCharts properties
    [ObservableProperty]
    private SeriesCollection _revenueSeries = new();

    [ObservableProperty]
    private SeriesCollection _topProductsSeries = new();

    [ObservableProperty]
    private string[] _revenueLabels = Array.Empty<string>();

    public Func<double, string> YFormatter => value => $"{value / 1000:N0}k";

    public DashboardViewModel(
        IDashboardService dashboardService,
        ISaleService saleService,
        IProductService productService)
    {
        _dashboardService = dashboardService;
        _saleService = saleService;
        _productService = productService;

        // Load data on construction
        _ = LoadDataAsync();
    }

    private async Task LoadDataAsync()
    {
        try
        {
            IsLoading = true;

            // Load stats
            TodayRevenue = await _dashboardService.GetTodayRevenueAsync();
            TodaySalesCount = await _dashboardService.GetTodaySalesCountAsync();
            LowStockCount = await _dashboardService.GetLowStockProductCountAsync();

            // Load products count
            var products = await _productService.GetAllProductsAsync();
            TotalProducts = products.Count();

            // Load recent sales
            var today = DateTime.Today;
            var sales = await _saleService.GetSalesByDateRangeAsync(today.AddDays(-7), today.AddDays(1));
            RecentSales = sales.Take(10).ToList();

            // Load low stock products
            var lowStock = await _productService.GetLowStockProductsAsync();
            LowStockProducts = lowStock.Take(5).ToList();

            // Load charts data
            await LoadChartsDataAsync();
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

    private async Task LoadChartsDataAsync()
    {
        try
        {
            // Get revenue for last 7 days
            var revenueData = new ChartValues<decimal>();
            var labels = new List<string>();
            
            for (int i = 6; i >= 0; i--)
            {
                var date = DateTime.Today.AddDays(-i);
                var daySales = await _saleService.GetSalesByDateRangeAsync(date, date.AddDays(1));
                revenueData.Add(daySales.Sum(s => s.TotalAmount));
                labels.Add(date.ToString("dd/MM"));
            }

            // Revenue Line Chart
            RevenueSeries = new SeriesCollection
            {
                new LineSeries
                {
                    Title = "Doanh thu",
                    Values = revenueData,
                    PointGeometry = DefaultGeometries.Circle,
                    Fill = new SolidColorBrush(Color.FromArgb(100, 30, 144, 255)),
                    Stroke = new SolidColorBrush(Colors.DodgerBlue),
                    StrokeThickness = 3
                }
            };

            RevenueLabels = labels.ToArray();

            // Top Products Pie Chart
            var topProducts = await _dashboardService.GetTopSellingProductsAsync(5);
            var pieColors = new Color[]
            {
                Colors.DodgerBlue,
                Colors.MediumSeaGreen,
                Colors.Orange,
                Colors.MediumPurple,
                Colors.Coral
            };

            TopProductsSeries = new SeriesCollection();
            var colorIndex = 0;
            foreach (var p in topProducts)
            {
                TopProductsSeries.Add(new PieSeries
                {
                    Title = p.ProductName.Length > 15 ? p.ProductName[..15] + "..." : p.ProductName,
                    Values = new ChartValues<decimal> { p.TotalRevenue },
                    Fill = new SolidColorBrush(pieColors[colorIndex % pieColors.Length]),
                    DataLabels = true
                });
                colorIndex++;
            }
        }
        catch
        {
            // If charts fail, still show dashboard without charts
        }
    }

    [RelayCommand]
    private async Task RefreshAsync()
    {
        await LoadDataAsync();
    }
}

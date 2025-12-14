using System.Windows.Controls;
using FPTMart.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace FPTMart.Views;

public partial class StockInView : UserControl
{
    public StockInView()
    {
        InitializeComponent();
        DataContext = App.ServiceProvider.GetRequiredService<StockInViewModel>();
    }
}

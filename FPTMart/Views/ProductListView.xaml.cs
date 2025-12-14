using System.Windows.Controls;
using FPTMart.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace FPTMart.Views;

public partial class ProductListView : UserControl
{
    public ProductListView()
    {
        InitializeComponent();
        DataContext = App.ServiceProvider.GetRequiredService<ProductListViewModel>();
    }
}

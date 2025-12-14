using System.Windows.Controls;
using FPTMart.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace FPTMart.Views;

public partial class CustomerListView : UserControl
{
    public CustomerListView()
    {
        InitializeComponent();
        DataContext = App.ServiceProvider.GetRequiredService<CustomerListViewModel>();
    }
}

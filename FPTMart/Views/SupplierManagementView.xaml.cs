using System.Windows.Controls;
using FPTMart.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace FPTMart.Views;

public partial class SupplierManagementView : UserControl
{
    public SupplierManagementView()
    {
        InitializeComponent();
        DataContext = App.ServiceProvider.GetRequiredService<SupplierManagementViewModel>();
    }
}

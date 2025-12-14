using System.Windows.Controls;
using FPTMart.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace FPTMart.Views;

public partial class POSView : UserControl
{
    public POSView()
    {
        InitializeComponent();
        DataContext = App.ServiceProvider.GetRequiredService<POSViewModel>();
    }
}

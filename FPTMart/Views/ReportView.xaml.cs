using System.Windows.Controls;
using FPTMart.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace FPTMart.Views;

public partial class ReportView : UserControl
{
    public ReportView()
    {
        InitializeComponent();
        DataContext = App.ServiceProvider.GetRequiredService<ReportViewModel>();
    }
}

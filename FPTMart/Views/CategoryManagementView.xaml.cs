using System.Windows.Controls;
using FPTMart.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace FPTMart.Views;

public partial class CategoryManagementView : UserControl
{
    public CategoryManagementView()
    {
        InitializeComponent();
        DataContext = App.ServiceProvider.GetRequiredService<CategoryManagementViewModel>();
    }
}

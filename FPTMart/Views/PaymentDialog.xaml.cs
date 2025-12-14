using System.Windows;
using FPTMart.ViewModels;

namespace FPTMart.Views;

public partial class PaymentDialog : Window
{
    public PaymentDialog(decimal totalAmount)
    {
        InitializeComponent();
        
        var viewModel = new PaymentDialogViewModel(totalAmount);
        viewModel.RequestClose += (result) =>
        {
            DialogResult = result;
            Close();
        };
        
        DataContext = viewModel;
    }

    public decimal PaidAmount => (DataContext as PaymentDialogViewModel)?.CashReceived ?? 0;
    public decimal ChangeAmount => (DataContext as PaymentDialogViewModel)?.ChangeAmount ?? 0;
    public string PaymentMethod => (DataContext as PaymentDialogViewModel)?.IsCashPayment == true ? "Cash" : "Transfer";
}

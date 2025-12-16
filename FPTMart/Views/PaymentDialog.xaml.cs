using System.Windows;
using FPTMart.ViewModels;

namespace FPTMart.Views;

public partial class PaymentDialog : Window
{
    private decimal _totalAmount;
    
    public PaymentDialog(decimal totalAmount)
    {
        InitializeComponent();
        _totalAmount = totalAmount;
        
        var viewModel = new PaymentDialogViewModel(totalAmount);
        viewModel.RequestClose += (result) =>
        {
            if (result && !viewModel.IsCashPayment)
            {
                // Show QR dialog for bank transfer
                var qrDialog = new QRBankingDialog(totalAmount);
                qrDialog.Owner = this;
                if (qrDialog.ShowDialog() != true)
                {
                    return; // User cancelled QR dialog
                }
            }
            DialogResult = result;
            Close();
        };
        
        DataContext = viewModel;
    }

    public decimal PaidAmount => (DataContext as PaymentDialogViewModel)?.CashReceived ?? 0;
    public decimal ChangeAmount => (DataContext as PaymentDialogViewModel)?.ChangeAmount ?? 0;
    public string PaymentMethod => (DataContext as PaymentDialogViewModel)?.IsCashPayment == true ? "Cash" : "Transfer";
}

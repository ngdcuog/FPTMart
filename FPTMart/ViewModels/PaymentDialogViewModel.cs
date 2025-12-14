using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Windows.Media;

namespace FPTMart.ViewModels;

public partial class PaymentDialogViewModel : ObservableObject
{
    public event Action<bool>? RequestClose;

    [ObservableProperty]
    private decimal _totalAmount;

    [ObservableProperty]
    private decimal _cashReceived;

    [ObservableProperty]
    private decimal _changeAmount;

    [ObservableProperty]
    private bool _isCashPayment = true;

    [ObservableProperty]
    private bool _canConfirm;

    [ObservableProperty]
    private Brush _changeBackground = new SolidColorBrush(Color.FromRgb(245, 245, 245));

    [ObservableProperty]
    private Brush _changeForeground = new SolidColorBrush(Colors.Gray);

    public PaymentDialogViewModel(decimal totalAmount)
    {
        TotalAmount = totalAmount;
        CashReceived = totalAmount;
        CalculateChange();
    }

    partial void OnCashReceivedChanged(decimal value)
    {
        CalculateChange();
    }

    private void CalculateChange()
    {
        ChangeAmount = CashReceived - TotalAmount;
        
        if (ChangeAmount >= 0)
        {
            CanConfirm = true;
            ChangeBackground = new SolidColorBrush(Color.FromRgb(232, 245, 233)); // Light green
            ChangeForeground = new SolidColorBrush(Color.FromRgb(56, 142, 60)); // Green
        }
        else
        {
            CanConfirm = false;
            ChangeBackground = new SolidColorBrush(Color.FromRgb(255, 235, 238)); // Light red
            ChangeForeground = new SolidColorBrush(Color.FromRgb(211, 47, 47)); // Red
        }
    }

    [RelayCommand]
    private void SetCash(string amount)
    {
        if (decimal.TryParse(amount, out var value))
        {
            CashReceived = value;
        }
    }

    [RelayCommand]
    private void SetExact()
    {
        CashReceived = TotalAmount;
    }

    [RelayCommand]
    private void Confirm()
    {
        if (CanConfirm)
        {
            RequestClose?.Invoke(true);
        }
    }

    [RelayCommand]
    private void Cancel()
    {
        RequestClose?.Invoke(false);
    }
}

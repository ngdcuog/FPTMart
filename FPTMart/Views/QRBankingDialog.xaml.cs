using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;

namespace FPTMart.Views;

public partial class QRBankingDialog : Window
{
    public QRBankingDialog(decimal amount)
    {
        InitializeComponent();
        
        AmountText.Text = $"{amount:N0} đ";
        
        // Load QR image
        LoadQRImage();
    }

    private void LoadQRImage()
    {
        try
        {
            // Path to QR image
            var qrPath = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory, 
                "..", "..", "..", "..", 
                "FPTMart.DAL", "Data", "QR_Banking", "qr_chuyen_khoan.png");
            
            qrPath = Path.GetFullPath(qrPath);
            
            if (File.Exists(qrPath))
            {
                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = new Uri(qrPath, UriKind.Absolute);
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.EndInit();
                bitmap.Freeze();
                
                QRImage.Source = bitmap;
            }
            else
            {
                MessageBox.Show($"Không tìm thấy file QR: {qrPath}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Lỗi load QR: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void Cancel_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }

    private void Confirm_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = true;
        Close();
    }
}

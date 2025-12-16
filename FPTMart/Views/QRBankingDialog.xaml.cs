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
            // Try multiple paths to find QR image
            var possiblePaths = new[]
            {
                // When running from VS (bin/Debug/net9.0-windows/)
                Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "..", "FPTMart.DAL", "Data", "QR_Banking", "qr_chuyen_khoan.png")),
                // Direct path from solution root
                Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "..", "..", "FPTMart.DAL", "Data", "QR_Banking", "qr_chuyen_khoan.png")),
                // Absolute fallback
                @"D:\1.FPT\Semester_5\PRN212_GiaoLang\FPTMart\FPTMart.DAL\Data\QR_Banking\qr_chuyen_khoan.png"
            };

            string? foundPath = null;
            foreach (var path in possiblePaths)
            {
                if (File.Exists(path))
                {
                    foundPath = path;
                    break;
                }
            }
            
            if (foundPath != null)
            {
                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = new Uri(foundPath, UriKind.Absolute);
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.EndInit();
                bitmap.Freeze();
                
                QRImage.Source = bitmap;
            }
            else
            {
                MessageBox.Show($"Không tìm thấy file QR.\nĐã thử:\n{string.Join("\n", possiblePaths)}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Warning);
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

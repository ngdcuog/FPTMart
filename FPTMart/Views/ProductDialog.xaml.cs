using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;
using FPTMart.BLL.DTOs;
using FPTMart.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Win32;

namespace FPTMart.Views;

public partial class ProductDialog : Window
{
    private ProductDialogViewModel _viewModel;

    public ProductDialog(ProductDto? product = null)
    {
        InitializeComponent();
        
        _viewModel = App.ServiceProvider.GetRequiredService<ProductDialogViewModel>();
        _viewModel.Initialize(product);
        _viewModel.RequestClose += (result) =>
        {
            DialogResult = result;
            Close();
        };
        
        DataContext = _viewModel;

        // Load existing image if any
        Loaded += (s, e) => LoadProductImage();
    }

    public ProductDto? Result => _viewModel?.Product;

    private void LoadProductImage()
    {
        if (!string.IsNullOrEmpty(_viewModel.Product?.ImagePath))
        {
            try
            {
                var imagePath = GetFullImagePath(_viewModel.Product.ImagePath);
                if (File.Exists(imagePath))
                {
                    var bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.UriSource = new Uri(imagePath, UriKind.Absolute);
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.EndInit();
                    ProductImage.Source = bitmap;
                    ImagePlaceholder.Visibility = Visibility.Collapsed;
                }
            }
            catch { }
        }
    }

    private void SelectImage_Click(object sender, RoutedEventArgs e)
    {
        var openFileDialog = new OpenFileDialog
        {
            Title = "Chọn ảnh sản phẩm",
            Filter = "Image files (*.jpg, *.jpeg, *.png, *.gif)|*.jpg;*.jpeg;*.png;*.gif|All files (*.*)|*.*",
            InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures)
        };

        if (openFileDialog.ShowDialog() == true)
        {
            try
            {
                var sourceFile = openFileDialog.FileName;
                var fileName = $"{Guid.NewGuid()}{Path.GetExtension(sourceFile)}";
                var destFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "Products");
                
                // Ensure directory exists
                Directory.CreateDirectory(destFolder);
                
                var destPath = Path.Combine(destFolder, fileName);
                
                // Copy file
                File.Copy(sourceFile, destPath, true);
                
                // Update DTO with just filename (not path)
                _viewModel.Product.ImagePath = fileName;
                
                // Display image
                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = new Uri(destPath, UriKind.Absolute);
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.EndInit();
                ProductImage.Source = bitmap;
                ImagePlaceholder.Visibility = Visibility.Collapsed;
                
                MessageBox.Show("Đã chọn ảnh thành công!", "Thông Báo", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi chọn ảnh: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    private void RemoveImage_Click(object sender, RoutedEventArgs e)
    {
        _viewModel.Product.ImagePath = null;
        ProductImage.Source = null;
        ImagePlaceholder.Visibility = Visibility.Visible;
    }

    private void ScanBarcode_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var scannerDialog = new BarcodeScannerDialog();
            scannerDialog.Owner = this;
            
            if (scannerDialog.ShowDialog() == true && !string.IsNullOrEmpty(scannerDialog.ScannedBarcode))
            {
                _viewModel.Product.Barcode = scannerDialog.ScannedBarcode;
                
                // Update TextBox directly since DTO doesn't have INotifyPropertyChanged
                var barcodeTextBox = FindName("BarcodeTextBox") as System.Windows.Controls.TextBox;
                if (barcodeTextBox != null)
                {
                    barcodeTextBox.Text = scannerDialog.ScannedBarcode;
                }
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Lỗi quét mã vạch: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private string GetFullImagePath(string relativePath)
    {
        return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, relativePath.Replace("/", "\\"));
    }
}

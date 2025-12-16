using FPTMart.BLL.DTOs;
using Microsoft.Win32;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace FPTMart.Views;

public partial class InvoiceDialog : Window
{
    public InvoiceDialog(SaleDto sale)
    {
        InitializeComponent();
        DataContext = new InvoiceViewModel(sale);
    }

    private void SaveImage_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            // Create directory if not exists
            var invoicesDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "..", "..", 
                "FPTMart.DAL", "Data", "Invoices");
            invoicesDir = Path.GetFullPath(invoicesDir);
            Directory.CreateDirectory(invoicesDir);

            // Generate filename
            var viewModel = DataContext as InvoiceViewModel;
            var fileName = $"{viewModel?.InvoiceNumber ?? "HD"}.png";
            var filePath = Path.Combine(invoicesDir, fileName);

            // Render to bitmap
            var renderBitmap = new RenderTargetBitmap(
                (int)InvoiceContent.ActualWidth,
                (int)InvoiceContent.ActualHeight,
                96, 96, PixelFormats.Pbgra32);

            InvoiceContent.Measure(new Size((int)InvoiceContent.ActualWidth, (int)InvoiceContent.ActualHeight));
            InvoiceContent.Arrange(new Rect(new Size((int)InvoiceContent.ActualWidth, (int)InvoiceContent.ActualHeight)));
            renderBitmap.Render(InvoiceContent);

            // Encode as PNG
            var encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(renderBitmap));

            // Save to file
            using (var stream = File.Create(filePath))
            {
                encoder.Save(stream);
            }

            MessageBox.Show(
                $"Đã lưu hóa đơn thành công!\n\nFile: {fileName}\nĐường dẫn: {invoicesDir}",
                "Thành Công",
                MessageBoxButton.OK,
                MessageBoxImage.Information);

            // Open folder
            System.Diagnostics.Process.Start("explorer.exe", invoicesDir);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Lỗi khi lưu ảnh: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void Close_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = true;
        Close();
    }
}

public class InvoiceViewModel
{
    public string InvoiceNumber { get; set; }
    public DateTime SaleDate { get; set; }
    public string? CustomerName { get; set; }
    public decimal SubTotal { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal PaidAmount { get; set; }
    public decimal ChangeAmount { get; set; }
    public bool HasDiscount => DiscountAmount > 0;
    public List<SaleItemDto> Items { get; set; }

    public InvoiceViewModel(SaleDto sale)
    {
        InvoiceNumber = sale.InvoiceNumber;
        SaleDate = sale.SaleDate;
        CustomerName = sale.CustomerName;
        SubTotal = sale.SubTotal;
        DiscountAmount = sale.DiscountAmount;
        TotalAmount = sale.TotalAmount;
        PaidAmount = sale.PaidAmount;
        ChangeAmount = sale.ChangeAmount;
        Items = sale.Items;
    }
}

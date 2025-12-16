using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace FPTMart.Converters;

/// <summary>
/// Converts null/empty string to Visibility.Collapsed, non-null to Visibility.Visible
/// </summary>
public class NullToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value == null)
            return Visibility.Collapsed;

        if (value is string str && string.IsNullOrEmpty(str))
            return Visibility.Collapsed;

        return Visibility.Visible;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// Inverts boolean to visibility - true = Collapsed, false = Visible
/// </summary>
public class InverseBoolToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool b)
            return b ? Visibility.Collapsed : Visibility.Visible;
        return Visibility.Visible;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// Converts product image filename to full path and loads as BitmapImage
/// </summary>
public class ProductImageConverter : IValueConverter
{
    // Base path for product images - adjust as needed
    private static readonly string ImageBasePath = Path.Combine(
        AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "..", 
        "FPTMart.DAL", "Data", "Products");

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value == null || string.IsNullOrEmpty(value.ToString()))
            return null!;

        try
        {
            var filename = value.ToString()!;
            var fullPath = Path.GetFullPath(Path.Combine(ImageBasePath, filename));
            
            if (!File.Exists(fullPath))
            {
                // Try alternative path in bin/Data/Products
                var altPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "Products", filename);
                if (File.Exists(altPath))
                    fullPath = altPath;
                else
                    return null!;
            }

            var bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.UriSource = new Uri(fullPath, UriKind.Absolute);
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.DecodePixelWidth = 100; // Optimize memory
            bitmap.EndInit();
            bitmap.Freeze();
            return bitmap;
        }
        catch
        {
            return null!;
        }
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// Converts English role names to Vietnamese
/// </summary>
public class RoleToVietnameseConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is string role)
        {
            return role switch
            {
                "Admin" => "Quản trị viên",
                "Manager" => "Quản lý",
                "Cashier" => "Thu ngân",
                "StockKeeper" => "Thủ kho",
                _ => role
            };
        }
        return value;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

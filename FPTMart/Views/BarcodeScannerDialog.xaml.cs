using System.Drawing;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using AForge.Video;
using AForge.Video.DirectShow;
using ZXing;
using ZXing.Common;
using ZXing.Windows.Compatibility;

namespace FPTMart.Views;

public partial class BarcodeScannerDialog : Window
{
    private FilterInfoCollection? _videoDevices;
    private VideoCaptureDevice? _videoSource;
    private readonly DispatcherTimer _scanTimer;
    private bool _isScanning = true;

    public string? ScannedBarcode { get; private set; }

    public BarcodeScannerDialog()
    {
        InitializeComponent();

        _scanTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(200)
        };

        Loaded += OnLoaded;
        Closing += OnClosing;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        try
        {
            // Get available cameras
            _videoDevices = new FilterInfoCollection(FilterCategory.VideoInputDevice);

            if (_videoDevices.Count == 0)
            {
                NoCamera.Visibility = Visibility.Visible;
                return;
            }

            // Use first camera
            _videoSource = new VideoCaptureDevice(_videoDevices[0].MonikerString);
            _videoSource.NewFrame += VideoSource_NewFrame;
            _videoSource.Start();
        }
        catch (Exception ex)
        {
            NoCamera.Text = $"Lỗi camera: {ex.Message}";
            NoCamera.Visibility = Visibility.Visible;
        }
    }

    private Bitmap? _currentFrame;
    private readonly object _frameLock = new();

    private void VideoSource_NewFrame(object sender, NewFrameEventArgs eventArgs)
    {
        try
        {
            lock (_frameLock)
            {
                _currentFrame?.Dispose();
                _currentFrame = (Bitmap)eventArgs.Frame.Clone();
            }

            // Update UI on main thread
            Dispatcher.BeginInvoke(() =>
            {
                try
                {
                    lock (_frameLock)
                    {
                        if (_currentFrame != null)
                        {
                            CameraPreview.Source = BitmapToImageSource(_currentFrame);

                            // Try to decode barcode
                            if (_isScanning)
                            {
                                DecodeBarcode(_currentFrame);
                            }
                        }
                    }
                }
                catch { }
            });
        }
        catch { }
    }

    private void DecodeBarcode(Bitmap bitmap)
    {
        try
        {
            var reader = new BarcodeReader
            {
                AutoRotate = true,
                Options = new DecodingOptions
                {
                    TryHarder = true,
                    PossibleFormats = new List<BarcodeFormat>
                    {
                        BarcodeFormat.EAN_13,
                        BarcodeFormat.EAN_8,
                        BarcodeFormat.UPC_A,
                        BarcodeFormat.UPC_E,
                        BarcodeFormat.CODE_128,
                        BarcodeFormat.CODE_39,
                        BarcodeFormat.QR_CODE
                    }
                }
            };

            var result = reader.Decode(bitmap);
            if (result != null)
            {
                _isScanning = false;
                ScannedBarcode = result.Text;
                
                // Play sound feedback
                System.Media.SystemSounds.Beep.Play();
                
                // Stop camera on background thread to avoid freeze
                Task.Run(() =>
                {
                    StopCamera();
                    
                    // Close dialog on UI thread
                    Dispatcher.Invoke(() =>
                    {
                        DialogResult = true;
                        Close();
                    });
                });
            }
        }
        catch { }
    }

    private static BitmapImage BitmapToImageSource(Bitmap bitmap)
    {
        using var memory = new MemoryStream();
        bitmap.Save(memory, System.Drawing.Imaging.ImageFormat.Bmp);
        memory.Position = 0;
        var bitmapImage = new BitmapImage();
        bitmapImage.BeginInit();
        bitmapImage.StreamSource = memory;
        bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
        bitmapImage.EndInit();
        bitmapImage.Freeze();
        return bitmapImage;
    }

    private void StopCamera()
    {
        if (_videoSource != null && _videoSource.IsRunning)
        {
            _videoSource.SignalToStop();
            _videoSource.WaitForStop();
            _videoSource = null;
        }

        lock (_frameLock)
        {
            _currentFrame?.Dispose();
            _currentFrame = null;
        }
    }

    private void UseBarcode_Click(object sender, RoutedEventArgs e)
    {
        if (!string.IsNullOrEmpty(ScannedBarcode))
        {
            DialogResult = true;
            Close();
        }
    }

    private void Cancel_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }

    private void ManualInput_Click(object sender, RoutedEventArgs e)
    {
        // Simple input dialog using a new window approach
        var inputWindow = new Window
        {
            Title = "Nhập Thủ Công",
            Width = 350,
            Height = 150,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            Owner = this,
            ResizeMode = ResizeMode.NoResize
        };

        var textBox = new System.Windows.Controls.TextBox
        {
            Margin = new Thickness(15),
            VerticalAlignment = VerticalAlignment.Center
        };

        var okButton = new System.Windows.Controls.Button
        {
            Content = "OK",
            Width = 80,
            Height = 30,
            Margin = new Thickness(0, 0, 15, 15),
            HorizontalAlignment = HorizontalAlignment.Right,
            VerticalAlignment = VerticalAlignment.Bottom
        };

        var grid = new System.Windows.Controls.Grid();
        grid.RowDefinitions.Add(new System.Windows.Controls.RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
        grid.RowDefinitions.Add(new System.Windows.Controls.RowDefinition { Height = GridLength.Auto });
        
        System.Windows.Controls.Grid.SetRow(textBox, 0);
        System.Windows.Controls.Grid.SetRow(okButton, 1);
        grid.Children.Add(textBox);
        grid.Children.Add(okButton);

        inputWindow.Content = grid;

        okButton.Click += (s, args) =>
        {
            if (!string.IsNullOrWhiteSpace(textBox.Text))
            {
                ScannedBarcode = textBox.Text.Trim();
                inputWindow.DialogResult = true;
            }
            inputWindow.Close();
        };

        if (inputWindow.ShowDialog() == true)
        {
            DialogResult = true;
            Close();
        }
    }

    private void OnClosing(object? sender, System.ComponentModel.CancelEventArgs e)
    {
        StopCamera();
    }
}

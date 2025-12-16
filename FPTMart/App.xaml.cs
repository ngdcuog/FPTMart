using System.IO;
using System.Windows;
using FPTMart.BLL.Services;
using FPTMart.DAL.Data;
using FPTMart.DAL.Entities;
using FPTMart.DAL.Repositories;
using FPTMart.ViewModels;
using FPTMart.Views;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FPTMart;

public partial class App : Application
{
    private readonly IServiceProvider _serviceProvider;
    public static IServiceProvider ServiceProvider { get; private set; } = null!;

    public App()
    {
        // Global exception handlers - prevent silent crash
        DispatcherUnhandledException += (s, e) =>
        {
            MessageBox.Show($"Lỗi ứng dụng:\n{e.Exception.Message}\n\nChi tiết:\n{e.Exception.StackTrace}", 
                "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            e.Handled = true;
        };
        
        AppDomain.CurrentDomain.UnhandledException += (s, e) =>
        {
            if (e.ExceptionObject is Exception ex)
            {
                MessageBox.Show($"Lỗi nghiêm trọng:\n{ex.Message}", 
                    "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        };
        
        TaskScheduler.UnobservedTaskException += (s, e) =>
        {
            MessageBox.Show($"Lỗi bất đồng bộ:\n{e.Exception.Message}", 
                "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            e.SetObserved();
        };

        var services = new ServiceCollection();
        ConfigureServices(services);
        _serviceProvider = services.BuildServiceProvider();
        ServiceProvider = _serviceProvider;
    }

    private void ConfigureServices(IServiceCollection services)
    {
        // Configuration
        var configuration = new ConfigurationBuilder()
            .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();

        services.AddSingleton<IConfiguration>(configuration);

        // DbContext
        services.AddDbContext<FPTMartDbContext>(options =>
        {
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection"));
        });

        // Repositories
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Services
        services.AddScoped<IProductService, ProductService>();
        services.AddScoped<ICategoryService, CategoryService>();
        services.AddScoped<ICustomerService, CustomerService>();
        services.AddScoped<ISupplierService, SupplierService>();
        services.AddScoped<ISaleService, SaleService>();
        services.AddScoped<IStockService, StockService>();
        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<IUserService, UserService>();
        services.AddSingleton<IAuthService, AuthService>();
        services.AddScoped<IDashboardService, DashboardService>();

        // ViewModels
        services.AddTransient<LoginViewModel>();
        services.AddTransient<MainViewModel>();
        services.AddTransient<DashboardViewModel>();
        services.AddTransient<ProductListViewModel>();
        services.AddTransient<ProductDialogViewModel>();
        services.AddTransient<POSViewModel>();
        services.AddTransient<StockInViewModel>();
        services.AddTransient<CustomerListViewModel>();
        services.AddTransient<ReportViewModel>();
        services.AddTransient<UserManagementViewModel>();
        services.AddTransient<SupplierManagementViewModel>();
        services.AddTransient<CategoryManagementViewModel>();

        // Views
        services.AddTransient<LoginWindow>();
        services.AddTransient<DashboardView>();
        services.AddTransient<ProductListView>();
        services.AddTransient<POSView>();
        services.AddTransient<CustomerListView>();
        services.AddTransient<StockInView>();
        services.AddTransient<ReportView>();
        services.AddTransient<UserManagementView>();
        services.AddTransient<SupplierManagementView>();
        services.AddTransient<CategoryManagementView>();

        // Windows
        services.AddTransient<MainWindow>();
    }

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // Ensure directories exist
        EnsureDirectoriesExist();

        // Initialize database and seed data
        InitializeDatabase();

        // Start with Login Window
        var loginWindow = _serviceProvider.GetRequiredService<LoginWindow>();
        loginWindow.Show();
    }

    private void EnsureDirectoriesExist()
    {
        var basePath = AppDomain.CurrentDomain.BaseDirectory;
        Directory.CreateDirectory(Path.Combine(basePath, "Data", "Products"));
        Directory.CreateDirectory(Path.Combine(basePath, "Data", "Invoices"));
    }

    private void InitializeDatabase()
    {
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<FPTMartDbContext>();
            var config = scope.ServiceProvider.GetRequiredService<IConfiguration>();

            // Ensure database is created (creates tables from model)
            context.Database.EnsureCreated();

            // Check if admin account exists, if not create it
            EnsureAdminAccountExists(context, config);

            // Ensure basic data exists
            EnsureBasicDataExists(context);
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Lỗi kết nối database:\n\n{ex.Message}\n\nVui lòng kiểm tra connection string trong appsettings.json",
                "Lỗi Database",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            Shutdown();
        }
    }

    private void EnsureAdminAccountExists(FPTMartDbContext context, IConfiguration config)
    {
        // Get admin settings from config
        var adminUsername = config["AdminSettings:DefaultUsername"] ?? "admin";
        var adminPassword = config["AdminSettings:DefaultPassword"] ?? "admin123";
        var adminFullName = config["AdminSettings:DefaultFullName"] ?? "Administrator";

        // Check if Admin role exists
        var adminRole = context.Roles.FirstOrDefault(r => r.Name == "Admin");
        if (adminRole == null)
        {
            adminRole = new Role
            {
                Name = "Admin",
                Description = "Quản trị viên hệ thống"
            };
            context.Roles.Add(adminRole);
            context.SaveChanges();
        }

        // Check if Cashier role exists
        var cashierRole = context.Roles.FirstOrDefault(r => r.Name == "Cashier");
        if (cashierRole == null)
        {
            cashierRole = new Role
            {
                Name = "Cashier",
                Description = "Thu ngân"
            };
            context.Roles.Add(cashierRole);
            context.SaveChanges();
        }

        // Check if Manager role exists
        var managerRole = context.Roles.FirstOrDefault(r => r.Name == "Manager");
        if (managerRole == null)
        {
            managerRole = new Role
            {
                Name = "Manager",
                Description = "Quản lý cửa hàng"
            };
            context.Roles.Add(managerRole);
            context.SaveChanges();
        }

        // Check if admin user exists
        var adminUser = context.Users.FirstOrDefault(u => u.Username == adminUsername);
        if (adminUser == null)
        {
            adminUser = new User
            {
                Username = adminUsername,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(adminPassword),
                FullName = adminFullName,
                Email = "admin@fptmart.com",
                IsActive = true,
                MustChangePassword = false, // Admin doesn't need to change
                CreatedAt = DateTime.Now
            };
            context.Users.Add(adminUser);
            context.SaveChanges();

            // Add admin role to user
            context.UserRoles.Add(new UserRole
            {
                UserId = adminUser.Id,
                RoleId = adminRole.Id
            });
            context.SaveChanges();
        }
    }

    private void EnsureBasicDataExists(FPTMartDbContext context)
    {
        // Ensure at least one category exists
        if (!context.Categories.Any())
        {
            var categories = new[]
            {
                new Category { Name = "Thực Phẩm", Description = "Các loại thực phẩm", IsActive = true, CreatedAt = DateTime.Now },
                new Category { Name = "Đồ Uống", Description = "Nước ngọt, nước suối, trà", IsActive = true, CreatedAt = DateTime.Now },
                new Category { Name = "Bánh Kẹo", Description = "Bánh, kẹo, snack", IsActive = true, CreatedAt = DateTime.Now },
                new Category { Name = "Gia Dụng", Description = "Đồ dùng gia đình", IsActive = true, CreatedAt = DateTime.Now },
                new Category { Name = "Chăm Sóc Cá Nhân", Description = "Xà phòng, dầu gội, kem đánh răng", IsActive = true, CreatedAt = DateTime.Now }
            };
            context.Categories.AddRange(categories);
            context.SaveChanges();
        }

        // Ensure at least one supplier exists
        if (!context.Suppliers.Any())
        {
            var suppliers = new[]
            {
                new Supplier { Name = "Công ty TNHH ABC", ContactPerson = "Nguyễn Văn A", Phone = "0901234567", IsActive = true, CreatedAt = DateTime.Now },
                new Supplier { Name = "Nhà phân phối XYZ", ContactPerson = "Trần Thị B", Phone = "0902345678", IsActive = true, CreatedAt = DateTime.Now }
            };
            context.Suppliers.AddRange(suppliers);
            context.SaveChanges();
        }

        // Add some sample products if none exist
        if (!context.Products.Any())
        {
            var foodCategory = context.Categories.FirstOrDefault(c => c.Name == "Thực Phẩm");
            var drinkCategory = context.Categories.FirstOrDefault(c => c.Name == "Đồ Uống");

            if (foodCategory != null && drinkCategory != null)
            {
                var products = new[]
                {
                    new Product { ProductCode = "SP0001", Barcode = "8934563011017", Name = "Mì Hảo Hảo", CategoryId = foodCategory.Id, CostPrice = 3000, SellingPrice = 4500, StockQuantity = 100, MinStockLevel = 20, Unit = "Gói", IsActive = true, CreatedAt = DateTime.Now },
                    new Product { ProductCode = "SP0002", Barcode = "8935049500117", Name = "Coca Cola 330ml", CategoryId = drinkCategory.Id, CostPrice = 7000, SellingPrice = 10000, StockQuantity = 50, MinStockLevel = 10, Unit = "Lon", IsActive = true, CreatedAt = DateTime.Now },
                    new Product { ProductCode = "SP0003", Barcode = "8934588012518", Name = "Nước suối Aquafina 500ml", CategoryId = drinkCategory.Id, CostPrice = 3500, SellingPrice = 5000, StockQuantity = 80, MinStockLevel = 15, Unit = "Chai", IsActive = true, CreatedAt = DateTime.Now }
                };
                context.Products.AddRange(products);
                context.SaveChanges();
            }
        }
    }
}

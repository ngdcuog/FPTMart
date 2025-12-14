using FPTMart.DAL.Entities;
using Microsoft.EntityFrameworkCore;

namespace FPTMart.DAL.Data;

public class FPTMartDbContext : DbContext
{
    public FPTMartDbContext(DbContextOptions<FPTMartDbContext> options) : base(options)
    {
    }

    // DbSets
    public DbSet<Category> Categories { get; set; }
    public DbSet<Product> Products { get; set; }
    public DbSet<Customer> Customers { get; set; }
    public DbSet<Supplier> Suppliers { get; set; }
    public DbSet<Sale> Sales { get; set; }
    public DbSet<SaleItem> SaleItems { get; set; }
    public DbSet<StockIn> StockIns { get; set; }
    public DbSet<StockInItem> StockInItems { get; set; }
    public DbSet<InventoryAdjustment> InventoryAdjustments { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<Role> Roles { get; set; }
    public DbSet<UserRole> UserRoles { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ========== Product ==========
        modelBuilder.Entity<Product>(entity =>
        {
            // ProductCode is UNIQUE
            entity.HasIndex(p => p.ProductCode).IsUnique();
            
            // Barcode is NOT unique (can have duplicates from manufacturers)
            entity.HasIndex(p => p.Barcode);
            
            entity.HasOne(p => p.Category)
                  .WithMany(c => c.Products)
                  .HasForeignKey(p => p.CategoryId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // ========== Category ==========
        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasIndex(c => c.Name).IsUnique();
        });

        // ========== Customer ==========
        modelBuilder.Entity<Customer>(entity =>
        {
            entity.HasIndex(c => c.Phone).IsUnique();
        });

        // ========== User ==========
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasIndex(u => u.Username).IsUnique();
            entity.HasIndex(u => u.Email).IsUnique();
            entity.HasIndex(u => u.Phone).IsUnique();
        });

        // ========== Role ==========
        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasIndex(r => r.Name).IsUnique();
        });

        // ========== UserRole (Many-to-Many) ==========
        modelBuilder.Entity<UserRole>(entity =>
        {
            entity.HasKey(ur => new { ur.UserId, ur.RoleId });

            entity.HasOne(ur => ur.User)
                  .WithMany(u => u.UserRoles)
                  .HasForeignKey(ur => ur.UserId);

            entity.HasOne(ur => ur.Role)
                  .WithMany(r => r.UserRoles)
                  .HasForeignKey(ur => ur.RoleId);
        });

        // ========== Sale ==========
        modelBuilder.Entity<Sale>(entity =>
        {
            entity.HasIndex(s => s.InvoiceNumber).IsUnique();

            entity.HasOne(s => s.Customer)
                  .WithMany(c => c.Sales)
                  .HasForeignKey(s => s.CustomerId)
                  .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(s => s.User)
                  .WithMany(u => u.Sales)
                  .HasForeignKey(s => s.UserId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // ========== SaleItem ==========
        modelBuilder.Entity<SaleItem>(entity =>
        {
            entity.HasOne(si => si.Sale)
                  .WithMany(s => s.SaleItems)
                  .HasForeignKey(si => si.SaleId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(si => si.Product)
                  .WithMany(p => p.SaleItems)
                  .HasForeignKey(si => si.ProductId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // ========== StockIn ==========
        modelBuilder.Entity<StockIn>(entity =>
        {
            entity.HasIndex(s => s.StockInNumber).IsUnique();

            entity.HasOne(s => s.Supplier)
                  .WithMany(sup => sup.StockIns)
                  .HasForeignKey(s => s.SupplierId)
                  .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(s => s.User)
                  .WithMany(u => u.StockIns)
                  .HasForeignKey(s => s.UserId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // ========== StockInItem ==========
        modelBuilder.Entity<StockInItem>(entity =>
        {
            entity.HasOne(sii => sii.StockIn)
                  .WithMany(si => si.StockInItems)
                  .HasForeignKey(sii => sii.StockInId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(sii => sii.Product)
                  .WithMany(p => p.StockInItems)
                  .HasForeignKey(sii => sii.ProductId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // ========== InventoryAdjustment ==========
        modelBuilder.Entity<InventoryAdjustment>(entity =>
        {
            entity.HasOne(ia => ia.Product)
                  .WithMany(p => p.InventoryAdjustments)
                  .HasForeignKey(ia => ia.ProductId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(ia => ia.User)
                  .WithMany(u => u.InventoryAdjustments)
                  .HasForeignKey(ia => ia.UserId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // ========== Seed Data ==========
        SeedData(modelBuilder);
    }

    private void SeedData(ModelBuilder modelBuilder)
    {
        // Fixed date for seed data (EF Core doesn't allow DateTime.Now in migrations)
        var seedDate = new DateTime(2025, 12, 13);

        // Roles
        modelBuilder.Entity<Role>().HasData(
            new Role { Id = 1, Name = "Admin", Description = "Quản trị viên - toàn quyền" },
            new Role { Id = 2, Name = "Manager", Description = "Quản lý - xem báo cáo, quản lý kho" },
            new Role { Id = 3, Name = "Cashier", Description = "Thu ngân - bán hàng" },
            new Role { Id = 4, Name = "StockKeeper", Description = "Thủ kho - nhập kho, điều chỉnh" }
        );

        // Admin user (password: admin123)
        modelBuilder.Entity<User>().HasData(
            new User
            {
                Id = 1,
                Username = "admin",
                PasswordHash = "admin123", // TODO: Hash this in production
                FullName = "Administrator",
                IsActive = true,
                CreatedAt = seedDate
            }
        );

        // Assign Admin role to admin user
        modelBuilder.Entity<UserRole>().HasData(
            new UserRole { UserId = 1, RoleId = 1 }
        );

        // Categories
        modelBuilder.Entity<Category>().HasData(
            new Category { Id = 1, Name = "Đồ uống", Description = "Nước ngọt, nước suối, trà...", IsActive = true, CreatedAt = seedDate },
            new Category { Id = 2, Name = "Bánh kẹo", Description = "Bánh, kẹo, snack...", IsActive = true, CreatedAt = seedDate },
            new Category { Id = 3, Name = "Mì gói", Description = "Mì ăn liền, cháo gói...", IsActive = true, CreatedAt = seedDate },
            new Category { Id = 4, Name = "Sữa", Description = "Sữa tươi, sữa hộp, sữa chua...", IsActive = true, CreatedAt = seedDate },
            new Category { Id = 5, Name = "Gia vị", Description = "Muối, đường, nước mắm...", IsActive = true, CreatedAt = seedDate }
        );

        // Suppliers
        modelBuilder.Entity<Supplier>().HasData(
            new Supplier { Id = 1, Name = "Công ty TNHH ABC", ContactPerson = "Nguyễn Văn A", Phone = "0901234567", IsActive = true, CreatedAt = seedDate },
            new Supplier { Id = 2, Name = "Đại lý XYZ", ContactPerson = "Trần Thị B", Phone = "0907654321", IsActive = true, CreatedAt = seedDate }
        );

        // Sample Products
        modelBuilder.Entity<Product>().HasData(
            new Product { Id = 1, ProductCode = "SP001", Barcode = "8934588012345", Name = "Coca Cola 330ml", CategoryId = 1, CostPrice = 8000, SellingPrice = 12000, StockQuantity = 100, Unit = "Lon", IsActive = true, CreatedAt = seedDate },
            new Product { Id = 2, ProductCode = "SP002", Barcode = "8934588012346", Name = "Pepsi 330ml", CategoryId = 1, CostPrice = 8000, SellingPrice = 12000, StockQuantity = 80, Unit = "Lon", IsActive = true, CreatedAt = seedDate },
            new Product { Id = 3, ProductCode = "SP003", Barcode = "8934588012347", Name = "Mì Hảo Hảo", CategoryId = 3, CostPrice = 3500, SellingPrice = 5000, StockQuantity = 200, Unit = "Gói", IsActive = true, CreatedAt = seedDate },
            new Product { Id = 4, ProductCode = "SP004", Barcode = "8934588012348", Name = "Sữa Vinamilk 180ml", CategoryId = 4, CostPrice = 6000, SellingPrice = 8000, StockQuantity = 150, Unit = "Hộp", IsActive = true, CreatedAt = seedDate }
        );
    }
}


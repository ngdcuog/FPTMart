namespace FPTMart.DAL.Repositories;

/// <summary>
/// Unit of Work Interface - Manages transactions and provides access to repositories
/// </summary>
public interface IUnitOfWork : IDisposable
{
    // Repositories
    IProductRepository Products { get; }
    ICategoryRepository Categories { get; }
    ICustomerRepository Customers { get; }
    ISupplierRepository Suppliers { get; }
    ISaleRepository Sales { get; }
    IStockInRepository StockIns { get; }
    IInventoryAdjustmentRepository InventoryAdjustments { get; }
    IUserRepository Users { get; }
    IRoleRepository Roles { get; }
    IGenericRepository<Entities.UserRole> UserRoles { get; }

    // Save changes
    Task<int> SaveChangesAsync();
    int SaveChanges();
}

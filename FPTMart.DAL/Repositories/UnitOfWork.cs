using FPTMart.DAL.Data;
using FPTMart.DAL.Entities;

namespace FPTMart.DAL.Repositories;

/// <summary>
/// Unit of Work Implementation - Manages transactions and provides access to repositories
/// </summary>
public class UnitOfWork : IUnitOfWork
{
    private readonly FPTMartDbContext _context;
    
    // Lazy initialization for repositories
    private IProductRepository? _products;
    private ICategoryRepository? _categories;
    private ICustomerRepository? _customers;
    private ISupplierRepository? _suppliers;
    private ISaleRepository? _sales;
    private IStockInRepository? _stockIns;
    private IInventoryAdjustmentRepository? _inventoryAdjustments;
    private IUserRepository? _users;
    private IRoleRepository? _roles;
    private IGenericRepository<UserRole>? _userRoles;

    public UnitOfWork(FPTMartDbContext context)
    {
        _context = context;
    }

    // Repositories - Lazy load
    public IProductRepository Products => _products ??= new ProductRepository(_context);
    public ICategoryRepository Categories => _categories ??= new CategoryRepository(_context);
    public ICustomerRepository Customers => _customers ??= new CustomerRepository(_context);
    public ISupplierRepository Suppliers => _suppliers ??= new SupplierRepository(_context);
    public ISaleRepository Sales => _sales ??= new SaleRepository(_context);
    public IStockInRepository StockIns => _stockIns ??= new StockInRepository(_context);
    public IInventoryAdjustmentRepository InventoryAdjustments => _inventoryAdjustments ??= new InventoryAdjustmentRepository(_context);
    public IUserRepository Users => _users ??= new UserRepository(_context);
    public IRoleRepository Roles => _roles ??= new RoleRepository(_context);
    public IGenericRepository<UserRole> UserRoles => _userRoles ??= new GenericRepository<UserRole>(_context);

    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }

    public int SaveChanges()
    {
        return _context.SaveChanges();
    }

    public void Dispose()
    {
        _context.Dispose();
        GC.SuppressFinalize(this);
    }
}

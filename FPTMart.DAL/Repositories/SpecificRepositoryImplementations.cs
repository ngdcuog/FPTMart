using FPTMart.DAL.Data;
using FPTMart.DAL.Entities;
using Microsoft.EntityFrameworkCore;

namespace FPTMart.DAL.Repositories;

#region Product Repository
public class ProductRepository : GenericRepository<Product>, IProductRepository
{
    public ProductRepository(FPTMartDbContext context) : base(context) { }

    public async Task<Product?> GetByProductCodeAsync(string productCode)
    {
        return await _dbSet.FirstOrDefaultAsync(p => p.ProductCode == productCode);
    }

    public async Task<IEnumerable<Product>> GetByBarcodeAsync(string barcode)
    {
        // Returns multiple because barcode may not be unique
        return await _dbSet.Where(p => p.Barcode == barcode).ToListAsync();
    }

    public async Task<IEnumerable<Product>> GetByCategoryAsync(int categoryId)
    {
        return await _dbSet.Where(p => p.CategoryId == categoryId && p.IsActive).ToListAsync();
    }

    public async Task<IEnumerable<Product>> GetLowStockProductsAsync()
    {
        return await _dbSet.Where(p => p.StockQuantity <= p.MinStockLevel && p.IsActive).ToListAsync();
    }

    public async Task<IEnumerable<Product>> SearchAsync(string keyword)
    {
        keyword = keyword.ToLower();
        return await _dbSet
            .Where(p => p.Name.ToLower().Contains(keyword) ||
                        p.ProductCode.ToLower().Contains(keyword) ||
                        (p.Barcode != null && p.Barcode.Contains(keyword)))
            .ToListAsync();
    }
}
#endregion

#region Category Repository
public class CategoryRepository : GenericRepository<Category>, ICategoryRepository
{
    public CategoryRepository(FPTMartDbContext context) : base(context) { }

    public async Task<Category?> GetByNameAsync(string name)
    {
        return await _dbSet.FirstOrDefaultAsync(c => c.Name == name);
    }

    public async Task<IEnumerable<Category>> GetActiveCategoriesAsync()
    {
        return await _dbSet.Where(c => c.IsActive).ToListAsync();
    }
}
#endregion

#region Customer Repository
public class CustomerRepository : GenericRepository<Customer>, ICustomerRepository
{
    public CustomerRepository(FPTMartDbContext context) : base(context) { }

    public async Task<Customer?> GetByPhoneAsync(string phone)
    {
        return await _dbSet.FirstOrDefaultAsync(c => c.Phone == phone);
    }

    public async Task<IEnumerable<Customer>> SearchAsync(string keyword)
    {
        keyword = keyword.ToLower();
        return await _dbSet
            .Where(c => c.FullName.ToLower().Contains(keyword) ||
                        (c.Phone != null && c.Phone.Contains(keyword)))
            .ToListAsync();
    }
}
#endregion

#region Supplier Repository
public class SupplierRepository : GenericRepository<Supplier>, ISupplierRepository
{
    public SupplierRepository(FPTMartDbContext context) : base(context) { }

    public async Task<IEnumerable<Supplier>> GetActiveSuppliersAsync()
    {
        return await _dbSet.Where(s => s.IsActive).ToListAsync();
    }
}
#endregion

#region Sale Repository
public class SaleRepository : GenericRepository<Sale>, ISaleRepository
{
    public SaleRepository(FPTMartDbContext context) : base(context) { }

    public async Task<Sale?> GetByInvoiceNumberAsync(string invoiceNumber)
    {
        return await _dbSet.FirstOrDefaultAsync(s => s.InvoiceNumber == invoiceNumber);
    }

    public async Task<IEnumerable<Sale>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        return await _dbSet
            .Include(s => s.SaleItems)
            .Where(s => s.SaleDate >= startDate && s.SaleDate <= endDate)
            .OrderByDescending(s => s.SaleDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<Sale>> GetByCustomerAsync(int customerId)
    {
        return await _dbSet
            .Where(s => s.CustomerId == customerId)
            .OrderByDescending(s => s.SaleDate)
            .ToListAsync();
    }

    public async Task<Sale?> GetWithItemsAsync(int saleId)
    {
        return await _dbSet
            .Include(s => s.SaleItems)
            .Include(s => s.Customer)
            .FirstOrDefaultAsync(s => s.Id == saleId);
    }

    public async Task<string> GenerateInvoiceNumberAsync()
    {
        var today = DateTime.Now.ToString("yyyyMMdd");
        var prefix = $"HD-{today}-";
        
        var lastInvoice = await _dbSet
            .Where(s => s.InvoiceNumber.StartsWith(prefix))
            .OrderByDescending(s => s.InvoiceNumber)
            .FirstOrDefaultAsync();

        if (lastInvoice == null)
            return $"{prefix}001";

        var lastNumber = int.Parse(lastInvoice.InvoiceNumber.Substring(prefix.Length));
        return $"{prefix}{(lastNumber + 1):D3}";
    }
}
#endregion

#region StockIn Repository
public class StockInRepository : GenericRepository<StockIn>, IStockInRepository
{
    public StockInRepository(FPTMartDbContext context) : base(context) { }

    public async Task<StockIn?> GetWithItemsAsync(int stockInId)
    {
        return await _dbSet
            .Include(s => s.StockInItems)
                .ThenInclude(si => si.Product)
            .Include(s => s.Supplier)
            .FirstOrDefaultAsync(s => s.Id == stockInId);
    }

    public async Task<IEnumerable<StockIn>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        return await _dbSet
            .Where(s => s.StockInDate >= startDate && s.StockInDate <= endDate)
            .OrderByDescending(s => s.StockInDate)
            .ToListAsync();
    }

    public async Task<string> GenerateStockInNumberAsync()
    {
        var today = DateTime.Now.ToString("yyyyMMdd");
        var prefix = $"NK-{today}-";
        
        var lastStockIn = await _dbSet
            .Where(s => s.StockInNumber.StartsWith(prefix))
            .OrderByDescending(s => s.StockInNumber)
            .FirstOrDefaultAsync();

        if (lastStockIn == null)
            return $"{prefix}001";

        var lastNumber = int.Parse(lastStockIn.StockInNumber.Substring(prefix.Length));
        return $"{prefix}{(lastNumber + 1):D3}";
    }
}
#endregion

#region InventoryAdjustment Repository
public class InventoryAdjustmentRepository : GenericRepository<InventoryAdjustment>, IInventoryAdjustmentRepository
{
    public InventoryAdjustmentRepository(FPTMartDbContext context) : base(context) { }

    public async Task<IEnumerable<InventoryAdjustment>> GetByProductAsync(int productId)
    {
        return await _dbSet
            .Where(ia => ia.ProductId == productId)
            .OrderByDescending(ia => ia.AdjustmentDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<InventoryAdjustment>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        return await _dbSet
            .Where(ia => ia.AdjustmentDate >= startDate && ia.AdjustmentDate <= endDate)
            .OrderByDescending(ia => ia.AdjustmentDate)
            .ToListAsync();
    }
}
#endregion

#region User Repository
public class UserRepository : GenericRepository<User>, IUserRepository
{
    public UserRepository(FPTMartDbContext context) : base(context) { }

    public async Task<User?> GetByUsernameAsync(string username)
    {
        return await _dbSet.FirstOrDefaultAsync(u => u.Username == username);
    }

    public async Task<User?> GetWithRolesAsync(int userId)
    {
        return await _dbSet
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Id == userId);
    }
}
#endregion

#region Role Repository
public class RoleRepository : GenericRepository<Role>, IRoleRepository
{
    public RoleRepository(FPTMartDbContext context) : base(context) { }

    public async Task<Role?> GetByNameAsync(string name)
    {
        return await _dbSet.FirstOrDefaultAsync(r => r.Name == name);
    }
}
#endregion

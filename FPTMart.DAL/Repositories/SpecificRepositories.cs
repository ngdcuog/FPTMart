using FPTMart.DAL.Entities;

namespace FPTMart.DAL.Repositories;

public interface IProductRepository : IGenericRepository<Product>
{
    Task<Product?> GetByProductCodeAsync(string productCode);
    Task<IEnumerable<Product>> GetByBarcodeAsync(string barcode);
    Task<IEnumerable<Product>> GetByCategoryAsync(int categoryId);
    Task<IEnumerable<Product>> GetLowStockProductsAsync();
    Task<IEnumerable<Product>> SearchAsync(string keyword);
}

public interface ICategoryRepository : IGenericRepository<Category>
{
    Task<Category?> GetByNameAsync(string name);
    Task<IEnumerable<Category>> GetActiveCategoriesAsync();
}

public interface ICustomerRepository : IGenericRepository<Customer>
{
    Task<Customer?> GetByPhoneAsync(string phone);
    Task<IEnumerable<Customer>> SearchAsync(string keyword);
}

public interface ISupplierRepository : IGenericRepository<Supplier>
{
    Task<IEnumerable<Supplier>> GetActiveSuppliersAsync();
}

public interface ISaleRepository : IGenericRepository<Sale>
{
    Task<Sale?> GetByInvoiceNumberAsync(string invoiceNumber);
    Task<IEnumerable<Sale>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);
    Task<IEnumerable<Sale>> GetByCustomerAsync(int customerId);
    Task<Sale?> GetWithItemsAsync(int saleId);
    Task<string> GenerateInvoiceNumberAsync();
}

public interface IStockInRepository : IGenericRepository<StockIn>
{
    Task<StockIn?> GetWithItemsAsync(int stockInId);
    Task<IEnumerable<StockIn>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);
    Task<string> GenerateStockInNumberAsync();
}

public interface IInventoryAdjustmentRepository : IGenericRepository<InventoryAdjustment>
{
    Task<IEnumerable<InventoryAdjustment>> GetByProductAsync(int productId);
    Task<IEnumerable<InventoryAdjustment>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);
}

public interface IUserRepository : IGenericRepository<User>
{
    Task<User?> GetByUsernameAsync(string username);
    Task<User?> GetWithRolesAsync(int userId);
}

public interface IRoleRepository : IGenericRepository<Role>
{
    Task<Role?> GetByNameAsync(string name);
}

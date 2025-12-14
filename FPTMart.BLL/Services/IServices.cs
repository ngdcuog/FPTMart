using FPTMart.BLL.DTOs;

namespace FPTMart.BLL.Services;

public interface IProductService
{
    Task<IEnumerable<ProductDto>> GetAllProductsAsync();
    Task<ProductDto?> GetProductByIdAsync(int id);
    Task<ProductDto?> GetProductByCodeAsync(string productCode);
    Task<IEnumerable<ProductDto>> GetProductsByBarcodeAsync(string barcode);
    Task<IEnumerable<ProductDto>> SearchProductsAsync(string keyword);
    Task<IEnumerable<ProductDto>> GetLowStockProductsAsync();
    Task<ProductDto> CreateProductAsync(ProductDto productDto);
    Task<ProductDto> UpdateProductAsync(ProductDto productDto);
    Task<bool> DeleteProductAsync(int id);
    Task<string> GenerateProductCodeAsync();
}

public interface ICategoryService
{
    Task<IEnumerable<CategoryDto>> GetAllCategoriesAsync();
    Task<IEnumerable<CategoryDto>> GetActiveCategoriesAsync();
    Task<CategoryDto?> GetCategoryByIdAsync(int id);
    Task<CategoryDto> CreateCategoryAsync(CategoryDto categoryDto);
    Task<CategoryDto> UpdateCategoryAsync(CategoryDto categoryDto);
    Task<bool> DeleteCategoryAsync(int id);
}

public interface ICustomerService
{
    Task<IEnumerable<CustomerDto>> GetAllCustomersAsync();
    Task<CustomerDto?> GetCustomerByIdAsync(int id);
    Task<CustomerDto?> GetCustomerByPhoneAsync(string phone);
    Task<IEnumerable<CustomerDto>> SearchCustomersAsync(string keyword);
    Task<CustomerDto> CreateCustomerAsync(CustomerDto customerDto);
    Task<CustomerDto> UpdateCustomerAsync(CustomerDto customerDto);
}

public interface ISupplierService
{
    Task<IEnumerable<SupplierDto>> GetAllSuppliersAsync();
    Task<IEnumerable<SupplierDto>> GetActiveSuppliersAsync();
    Task<SupplierDto?> GetSupplierByIdAsync(int id);
    Task<SupplierDto> CreateSupplierAsync(SupplierDto supplierDto);
    Task<SupplierDto> UpdateSupplierAsync(SupplierDto supplierDto);
}

public interface ISaleService
{
    Task<IEnumerable<SaleDto>> GetAllSalesAsync();
    Task<SaleDto?> GetSaleByIdAsync(int id);
    Task<SaleDto?> GetSaleByInvoiceNumberAsync(string invoiceNumber);
    Task<IEnumerable<SaleDto>> GetSalesByDateRangeAsync(DateTime startDate, DateTime endDate);
    Task<SaleDto> CreateSaleAsync(SaleDto saleDto);
    Task<bool> CancelSaleAsync(int id);
    Task<decimal> GetTodayRevenueAsync();
    Task<int> GetTodaySalesCountAsync();
}

public interface IStockService
{
    // Stock In
    Task<IEnumerable<StockInDto>> GetAllStockInsAsync();
    Task<StockInDto?> GetStockInByIdAsync(int id);
    Task<StockInDto> CreateStockInAsync(StockInDto stockInDto);
    
    // Inventory Adjustment
    Task AdjustInventoryAsync(int productId, int quantityChange, string adjustmentType, string? reason, int userId);
}

public interface IAuthService
{
    Task<LoginResultDto> LoginAsync(LoginDto loginDto);
    Task<bool> ChangePasswordAsync(int userId, string oldPassword, string newPassword);
    UserDto? GetCurrentUser();
    void SetCurrentUser(UserDto? user);
    bool HasRole(string roleName);
}

public interface IDashboardService
{
    Task<decimal> GetTodayRevenueAsync();
    Task<int> GetTodaySalesCountAsync();
    Task<int> GetLowStockProductCountAsync();
    Task<IEnumerable<TopSellingProductDto>> GetTopSellingProductsAsync(int count = 5);
}

public interface IUserService
{
    Task<IEnumerable<UserDto>> GetAllUsersAsync();
    Task<UserDto?> GetUserByIdAsync(int id);
    Task<UserDto> CreateUserAsync(CreateUserDto userDto);
    Task<UserDto> UpdateUserAsync(UserDto userDto);
    Task<bool> DeactivateUserAsync(int id);
    Task<bool> ActivateUserAsync(int id);
    Task<bool> ResetPasswordAsync(int userId, string newPassword);
    Task<IEnumerable<RoleDto>> GetAllRolesAsync();
    Task<bool> AssignRoleAsync(int userId, int roleId);
    Task<bool> RemoveRoleAsync(int userId, int roleId);
}

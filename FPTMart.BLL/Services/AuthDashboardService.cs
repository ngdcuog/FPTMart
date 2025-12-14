using FPTMart.BLL.DTOs;
using FPTMart.DAL.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace FPTMart.BLL.Services;

/// <summary>
/// AuthService - Singleton with IServiceScopeFactory to properly handle scoped DbContext
/// </summary>
public class AuthService : IAuthService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private UserDto? _currentUser;

    public AuthService(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    public async Task<LoginResultDto> LoginAsync(LoginDto loginDto)
    {
        using var scope = _scopeFactory.CreateScope();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

        var user = await unitOfWork.Users.GetByUsernameAsync(loginDto.Username);
        
        if (user == null)
        {
            return new LoginResultDto
            {
                Success = false,
                Message = "Tên đăng nhập không tồn tại"
            };
        }

        if (!user.IsActive)
        {
            return new LoginResultDto
            {
                Success = false,
                Message = "Tài khoản đã bị vô hiệu hóa"
            };
        }

        // Verify password with BCrypt (fallback to plain text for migration)
        if (!PasswordHelper.VerifyPassword(loginDto.Password, user.PasswordHash))
        {
            return new LoginResultDto
            {
                Success = false,
                Message = "Mật khẩu không đúng"
            };
        }

        // Update last login
        user.LastLoginAt = DateTime.Now;
        unitOfWork.Users.Update(user);
        await unitOfWork.SaveChangesAsync();

        // Get user with roles
        var userWithRoles = await unitOfWork.Users.GetWithRolesAsync(user.Id);
        var userDto = new UserDto
        {
            Id = user.Id,
            Username = user.Username,
            FullName = user.FullName,
            Email = user.Email,
            Phone = user.Phone,
            IsActive = user.IsActive,
            MustChangePassword = user.MustChangePassword,
            Roles = userWithRoles?.UserRoles.Select(ur => ur.Role?.Name ?? "").ToList() ?? new List<string>()
        };

        _currentUser = userDto;

        return new LoginResultDto
        {
            Success = true,
            Message = user.MustChangePassword ? "Vui lòng đổi mật khẩu" : "Đăng nhập thành công",
            User = userDto,
            RequirePasswordChange = user.MustChangePassword
        };
    }

    public async Task<bool> ChangePasswordAsync(int userId, string oldPassword, string newPassword)
    {
        using var scope = _scopeFactory.CreateScope();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

        var user = await unitOfWork.Users.GetByIdAsync(userId);
        if (user == null) return false;

        // Verify old password
        if (!PasswordHelper.VerifyPassword(oldPassword, user.PasswordHash)) return false;

        // Hash and set new password
        user.PasswordHash = PasswordHelper.HashPassword(newPassword);
        user.MustChangePassword = false;
        user.UpdatedAt = DateTime.Now;
        unitOfWork.Users.Update(user);
        await unitOfWork.SaveChangesAsync();

        // Update current user
        if (_currentUser != null && _currentUser.Id == userId)
        {
            _currentUser.MustChangePassword = false;
        }

        return true;
    }

    public UserDto? GetCurrentUser() => _currentUser;

    public void SetCurrentUser(UserDto? user) => _currentUser = user;

    public bool HasRole(string roleName)
    {
        return _currentUser?.Roles.Contains(roleName) ?? false;
    }
}

public class DashboardService : IDashboardService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ISaleService _saleService;
    private readonly IProductService _productService;

    public DashboardService(IUnitOfWork unitOfWork, ISaleService saleService, IProductService productService)
    {
        _unitOfWork = unitOfWork;
        _saleService = saleService;
        _productService = productService;
    }

    public async Task<decimal> GetTodayRevenueAsync()
    {
        return await _saleService.GetTodayRevenueAsync();
    }

    public async Task<int> GetTodaySalesCountAsync()
    {
        return await _saleService.GetTodaySalesCountAsync();
    }

    public async Task<int> GetLowStockProductCountAsync()
    {
        var lowStockProducts = await _productService.GetLowStockProductsAsync();
        return lowStockProducts.Count();
    }

    public async Task<IEnumerable<TopSellingProductDto>> GetTopSellingProductsAsync(int count = 5)
    {
        // Get all sales from last 30 days
        var startDate = DateTime.Today.AddDays(-30);
        var sales = await _saleService.GetSalesByDateRangeAsync(startDate, DateTime.Today.AddDays(1));
        
        // Group by product and calculate totals
        var topProducts = sales
            .SelectMany(s => s.Items)
            .GroupBy(i => new { i.ProductId, i.ProductName })
            .Select(g => new TopSellingProductDto
            {
                ProductId = g.Key.ProductId,
                ProductName = g.Key.ProductName,
                QuantitySold = g.Sum(i => i.Quantity),
                TotalRevenue = g.Sum(i => i.TotalPrice)
            })
            .OrderByDescending(p => p.TotalRevenue)
            .Take(count);

        return topProducts;
    }
}

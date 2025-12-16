using System.ComponentModel;
using FPTMart.DAL.Entities;

namespace FPTMart.BLL.DTOs;

/// <summary>
/// Product DTO for data transfer
/// </summary>
public class ProductDto
{
    public int Id { get; set; }
    public string ProductCode { get; set; } = string.Empty;
    public string? Barcode { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int CategoryId { get; set; }
    public string? CategoryName { get; set; }
    public decimal CostPrice { get; set; }
    public decimal SellingPrice { get; set; }
    public int StockQuantity { get; set; }
    public int MinStockLevel { get; set; }
    public int UnitsPerCase { get; set; } = 1;
    public string CaseUnit { get; set; } = "Thùng";
    public string Unit { get; set; } = "Cái";
    public string? ImagePath { get; set; }
    public bool IsActive { get; set; }
    public bool IsLowStock => StockQuantity <= MinStockLevel;
    
    // Computed properties for display
    public int StockInCases => UnitsPerCase > 0 ? StockQuantity / UnitsPerCase : 0;
    public int StockRemainder => UnitsPerCase > 0 ? StockQuantity % UnitsPerCase : StockQuantity;
    public string StockDisplay => UnitsPerCase > 1 
        ? $"{StockInCases} {CaseUnit} + {StockRemainder} {Unit} ({StockQuantity} {Unit})"
        : $"{StockQuantity} {Unit}";
}

public class CategoryDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; }
    public int ProductCount { get; set; }
}

public class TopSellingProductDto
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int QuantitySold { get; set; }
    public decimal TotalRevenue { get; set; }
}

public class CustomerDto
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Name => FullName; // Alias for display
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }
    public decimal TotalPurchases { get; set; }
    public bool IsActive { get; set; } = true;
}

public class SupplierDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? ContactPerson { get; set; }
    public string? Phone { get; set; }
    public bool IsActive { get; set; } = true;
}

public class SaleDto
{
    public int Id { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public int? CustomerId { get; set; }
    public string? CustomerName { get; set; }
    public DateTime SaleDate { get; set; }
    public decimal SubTotal { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal PaidAmount { get; set; }
    public decimal ChangeAmount { get; set; }
    public string PaymentMethod { get; set; } = "Cash";
    public string Status { get; set; } = "Completed";
    public List<SaleItemDto> Items { get; set; } = new();
}

public class SaleItemDto : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;
    
    private void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public int ProductId { get; set; }
    public string ProductCode { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    
    private int _quantity;
    public int Quantity
    {
        get => _quantity;
        set
        {
            // Validate: min 1, max stock
            var newValue = Math.Max(1, Math.Min(value, MaxQuantity > 0 ? MaxQuantity : value));
            if (_quantity != newValue)
            {
                _quantity = newValue;
                TotalPrice = _quantity * UnitPrice; // Auto update total
                OnPropertyChanged(nameof(Quantity));
            }
        }
    }
    
    public decimal UnitPrice { get; set; }
    public decimal DiscountAmount { get; set; }
    
    private decimal _totalPrice;
    public decimal TotalPrice
    {
        get => _totalPrice;
        set
        {
            if (_totalPrice != value)
            {
                _totalPrice = value;
                OnPropertyChanged(nameof(TotalPrice));
            }
        }
    }
    
    public int MaxQuantity { get; set; } // For stock validation in POS
}

public class StockInDto
{
    public int Id { get; set; }
    public string StockInNumber { get; set; } = string.Empty;
    public int? SupplierId { get; set; }
    public string? SupplierName { get; set; }
    public DateTime StockInDate { get; set; }
    public decimal TotalAmount { get; set; }
    public string? Notes { get; set; }
    public List<StockInItemDto> Items { get; set; } = new();
    
    // Computed: total cases across all items
    public int TotalCases => Items.Sum(i => i.CaseQuantity);
}

public class StockInItemDto : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;
    
    private void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public int ProductId { get; set; }
    public string ProductCode { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    
    // Case/Unit configuration from Product
    public int UnitsPerCase { get; set; } = 1;
    public string CaseUnit { get; set; } = "Thùng";
    public string Unit { get; set; } = "Cái";
    
    // User input: number of cases
    private int _caseQuantity = 1;
    public int CaseQuantity
    {
        get => _caseQuantity;
        set
        {
            if (_caseQuantity != value)
            {
                _caseQuantity = value;
                OnPropertyChanged(nameof(CaseQuantity));
                OnPropertyChanged(nameof(Quantity));
                OnPropertyChanged(nameof(TotalPrice));
                OnPropertyChanged(nameof(ConversionDisplay));
            }
        }
    }
    
    // Computed: total units = CaseQuantity * UnitsPerCase
    public int Quantity => CaseQuantity * UnitsPerCase;
    
    // Cost per case (user input)
    private decimal _costPrice;
    public decimal CostPrice
    {
        get => _costPrice;
        set
        {
            if (_costPrice != value)
            {
                _costPrice = value;
                OnPropertyChanged(nameof(CostPrice));
                OnPropertyChanged(nameof(TotalPrice));
            }
        }
    }
    
    // Total = CaseQuantity * CostPrice
    public decimal TotalPrice => CaseQuantity * CostPrice;
    
    // Display helpers
    public string CaseInfo => UnitsPerCase > 1 ? $"Quy cách: {UnitsPerCase} {Unit}/{CaseUnit}" : "";
    public string ConversionDisplay => UnitsPerCase > 1 
        ? $"→ {CaseQuantity} {CaseUnit} = {Quantity} {Unit}" 
        : $"→ {Quantity} {Unit}";
}

public class UserDto
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public bool IsActive { get; set; }
    public bool MustChangePassword { get; set; }
    public List<string> Roles { get; set; } = new();
}

public class LoginDto
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class LoginResultDto
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public UserDto? User { get; set; }
    public bool RequirePasswordChange { get; set; }
}

public class CreateUserDto
{
    public string Username { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty; // Required for sending password
    public string? Phone { get; set; }
    public int RoleId { get; set; }
}

public class RoleDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    
    // Vietnamese display name
    public string DisplayName => Name switch
    {
        "Admin" => "Quản trị viên",
        "Manager" => "Quản lý",
        "Cashier" => "Thu ngân",
        "StockKeeper" => "Thủ kho",
        _ => Name
    };
}

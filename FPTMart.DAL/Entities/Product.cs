using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FPTMart.DAL.Entities;

/// <summary>
/// Sản phẩm
/// </summary>
public class Product
{
    [Key]
    public int Id { get; set; }

    /// <summary>
    /// Mã nội bộ cửa hàng (SP001, SP002...) - UNIQUE
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string ProductCode { get; set; } = string.Empty;

    /// <summary>
    /// Mã vạch nhà sản xuất (EAN-13, UPC...) - KHÔNG cần unique
    /// </summary>
    [MaxLength(50)]
    public string? Barcode { get; set; }

    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? Description { get; set; }

    public int CategoryId { get; set; }

    /// <summary>
    /// Giá nhập
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal CostPrice { get; set; }

    /// <summary>
    /// Giá bán
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal SellingPrice { get; set; }

    /// <summary>
    /// Số lượng tồn kho
    /// </summary>
    public int StockQuantity { get; set; } = 0;

    /// <summary>
    /// Mức tồn kho tối thiểu (cảnh báo khi dưới mức này)
    /// </summary>
    public int MinStockLevel { get; set; } = 10;

    /// <summary>
    /// Số đơn vị lẻ trong 1 thùng/kiện (VD: 24 lon/thùng)
    /// Dùng để quy đổi khi nhập kho theo thùng
    /// </summary>
    public int UnitsPerCase { get; set; } = 1;

    /// <summary>
    /// Đơn vị khi nhập kho (Thùng, Kiện, Hộp lớn...)
    /// </summary>
    [MaxLength(50)]
    public string CaseUnit { get; set; } = "Thùng";

    /// <summary>
    /// Đơn vị tính khi bán lẻ (Lon, Chai, Cái...)
    /// </summary>
    [MaxLength(50)]
    public string Unit { get; set; } = "Cái";

    /// <summary>
    /// Đường dẫn ảnh sản phẩm (relative path)
    /// </summary>
    [MaxLength(500)]
    public string? ImagePath { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public DateTime? UpdatedAt { get; set; }

    // Navigation properties
    [ForeignKey(nameof(CategoryId))]
    public virtual Category? Category { get; set; }

    public virtual ICollection<SaleItem> SaleItems { get; set; } = new List<SaleItem>();
    public virtual ICollection<StockInItem> StockInItems { get; set; } = new List<StockInItem>();
    public virtual ICollection<InventoryAdjustment> InventoryAdjustments { get; set; } = new List<InventoryAdjustment>();
}

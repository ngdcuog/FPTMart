using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FPTMart.DAL.Entities;

/// <summary>
/// Chi tiết đơn hàng
/// </summary>
public class SaleItem
{
    [Key]
    public int Id { get; set; }

    public int SaleId { get; set; }

    public int ProductId { get; set; }

    /// <summary>
    /// Tên SP lưu tại thời điểm bán (không bị ảnh hưởng khi đổi tên sau)
    /// </summary>
    [Required]
    [MaxLength(200)]
    public string ProductName { get; set; } = string.Empty;

    /// <summary>
    /// Mã SP lưu tại thời điểm bán
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string ProductCode { get; set; } = string.Empty;

    public int Quantity { get; set; }

    /// <summary>
    /// Đơn giá tại thời điểm bán
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal UnitPrice { get; set; }

    /// <summary>
    /// Giảm giá cho item này
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal DiscountAmount { get; set; } = 0;

    /// <summary>
    /// Thành tiền = Quantity * UnitPrice - DiscountAmount
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalPrice { get; set; }

    // Navigation properties
    [ForeignKey(nameof(SaleId))]
    public virtual Sale? Sale { get; set; }

    [ForeignKey(nameof(ProductId))]
    public virtual Product? Product { get; set; }
}

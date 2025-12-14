using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FPTMart.DAL.Entities;

/// <summary>
/// Điều chỉnh tồn kho (thay thế StockOut)
/// Dùng cho: hàng hư, hết hạn, mất mát, trả NCC...
/// </summary>
public class InventoryAdjustment
{
    [Key]
    public int Id { get; set; }

    public int ProductId { get; set; }

    /// <summary>
    /// Người điều chỉnh
    /// </summary>
    public int UserId { get; set; }

    /// <summary>
    /// Loại điều chỉnh: Damage, Expired, Lost, ReturnToSupplier, Correction, Gift
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string AdjustmentType { get; set; } = string.Empty;

    /// <summary>
    /// Số lượng thay đổi: âm = giảm, dương = tăng
    /// Ví dụ: -10 = giảm 10, +5 = tăng 5
    /// </summary>
    public int QuantityChange { get; set; }

    /// <summary>
    /// Lý do chi tiết
    /// </summary>
    [MaxLength(500)]
    public string? Reason { get; set; }

    public DateTime AdjustmentDate { get; set; } = DateTime.Now;

    public DateTime CreatedAt { get; set; } = DateTime.Now;

    // Navigation properties
    [ForeignKey(nameof(ProductId))]
    public virtual Product? Product { get; set; }

    [ForeignKey(nameof(UserId))]
    public virtual User? User { get; set; }
}

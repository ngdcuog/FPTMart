using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FPTMart.DAL.Entities;

/// <summary>
/// Phiếu nhập kho
/// </summary>
public class StockIn
{
    [Key]
    public int Id { get; set; }

    /// <summary>
    /// Số phiếu nhập (NK-20251213-001)
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string StockInNumber { get; set; } = string.Empty;

    /// <summary>
    /// Nhà cung cấp (optional)
    /// </summary>
    public int? SupplierId { get; set; }

    /// <summary>
    /// Người nhập kho
    /// </summary>
    public int UserId { get; set; }

    public DateTime StockInDate { get; set; } = DateTime.Now;

    /// <summary>
    /// Tổng tiền nhập
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalAmount { get; set; }

    [MaxLength(500)]
    public string? Notes { get; set; }

    /// <summary>
    /// Trạng thái: Completed, Cancelled
    /// </summary>
    [MaxLength(20)]
    public string Status { get; set; } = "Completed";

    public DateTime CreatedAt { get; set; } = DateTime.Now;

    // Navigation properties
    [ForeignKey(nameof(SupplierId))]
    public virtual Supplier? Supplier { get; set; }

    [ForeignKey(nameof(UserId))]
    public virtual User? User { get; set; }

    public virtual ICollection<StockInItem> StockInItems { get; set; } = new List<StockInItem>();
}

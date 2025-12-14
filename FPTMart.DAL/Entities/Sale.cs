using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FPTMart.DAL.Entities;

/// <summary>
/// Đơn hàng / Hóa đơn
/// </summary>
public class Sale
{
    [Key]
    public int Id { get; set; }

    /// <summary>
    /// Số hóa đơn (HD-20251213-001)
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string InvoiceNumber { get; set; } = string.Empty;

    /// <summary>
    /// Khách hàng (NULL = khách lẻ)
    /// </summary>
    public int? CustomerId { get; set; }

    /// <summary>
    /// Nhân viên bán hàng
    /// </summary>
    public int UserId { get; set; }

    public DateTime SaleDate { get; set; } = DateTime.Now;

    /// <summary>
    /// Tổng tiền trước giảm giá
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal SubTotal { get; set; }

    /// <summary>
    /// Số tiền giảm giá
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal DiscountAmount { get; set; } = 0;

    /// <summary>
    /// Phần trăm giảm giá
    /// </summary>
    [Column(TypeName = "decimal(5,2)")]
    public decimal DiscountPercent { get; set; } = 0;

    /// <summary>
    /// Tổng tiền phải trả
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalAmount { get; set; }

    /// <summary>
    /// Số tiền khách đưa
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal PaidAmount { get; set; }

    /// <summary>
    /// Tiền thừa trả lại
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal ChangeAmount { get; set; }

    /// <summary>
    /// Phương thức thanh toán: Cash, BankTransfer, Card, Momo
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string PaymentMethod { get; set; } = "Cash";

    /// <summary>
    /// Trạng thái: Completed, Cancelled, Refunded
    /// </summary>
    [MaxLength(20)]
    public string Status { get; set; } = "Completed";

    [MaxLength(500)]
    public string? Notes { get; set; }

    /// <summary>
    /// Đường dẫn ảnh hóa đơn (tạo on-demand khi user bấm tải)
    /// </summary>
    [MaxLength(500)]
    public string? InvoiceImagePath { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.Now;

    // Navigation properties
    [ForeignKey(nameof(CustomerId))]
    public virtual Customer? Customer { get; set; }

    [ForeignKey(nameof(UserId))]
    public virtual User? User { get; set; }

    public virtual ICollection<SaleItem> SaleItems { get; set; } = new List<SaleItem>();
}

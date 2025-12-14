using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FPTMart.DAL.Entities;

/// <summary>
/// Chi tiết phiếu nhập kho
/// </summary>
public class StockInItem
{
    [Key]
    public int Id { get; set; }

    public int StockInId { get; set; }

    public int ProductId { get; set; }

    /// <summary>
    /// Số lượng nhập
    /// </summary>
    public int Quantity { get; set; }

    /// <summary>
    /// Giá nhập / đơn vị
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal CostPrice { get; set; }

    /// <summary>
    /// Thành tiền = Quantity * CostPrice
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalPrice { get; set; }

    // Navigation properties
    [ForeignKey(nameof(StockInId))]
    public virtual StockIn? StockIn { get; set; }

    [ForeignKey(nameof(ProductId))]
    public virtual Product? Product { get; set; }
}

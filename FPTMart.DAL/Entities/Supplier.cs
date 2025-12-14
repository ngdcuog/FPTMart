using System.ComponentModel.DataAnnotations;

namespace FPTMart.DAL.Entities;

/// <summary>
/// Nhà cung cấp - Liên kết với StockIn, không liên kết với Product
/// </summary>
public class Supplier
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? ContactPerson { get; set; }

    [MaxLength(20)]
    public string? Phone { get; set; }

    [MaxLength(100)]
    public string? Email { get; set; }

    [MaxLength(500)]
    public string? Address { get; set; }

    [MaxLength(500)]
    public string? Notes { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public DateTime? UpdatedAt { get; set; }

    // Navigation property
    public virtual ICollection<StockIn> StockIns { get; set; } = new List<StockIn>();
}

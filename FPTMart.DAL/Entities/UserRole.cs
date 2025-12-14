using System.ComponentModel.DataAnnotations.Schema;

namespace FPTMart.DAL.Entities;

/// <summary>
/// Bảng trung gian User-Role (Many-to-Many)
/// </summary>
public class UserRole
{
    public int UserId { get; set; }
    public int RoleId { get; set; }

    // Navigation properties
    [ForeignKey(nameof(UserId))]
    public virtual User? User { get; set; }

    [ForeignKey(nameof(RoleId))]
    public virtual Role? Role { get; set; }
}

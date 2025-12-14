using System.ComponentModel.DataAnnotations;

namespace FPTMart.DAL.Entities;

/// <summary>
/// Vai trò người dùng
/// </summary>
public class Role
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(50)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(200)]
    public string? Description { get; set; }

    // Navigation property
    public virtual ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
}

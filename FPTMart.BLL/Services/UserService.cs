using FPTMart.BLL.DTOs;
using FPTMart.DAL.Entities;
using FPTMart.DAL.Repositories;
using System.Text.RegularExpressions;

namespace FPTMart.BLL.Services;

public class UserService : IUserService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEmailService _emailService;

    public UserService(IUnitOfWork unitOfWork, IEmailService emailService)
    {
        _unitOfWork = unitOfWork;
        _emailService = emailService;
    }

    public async Task<IEnumerable<UserDto>> GetAllUsersAsync()
    {
        var users = await _unitOfWork.Users.GetAllAsync();
        var userDtos = new List<UserDto>();

        foreach (var user in users)
        {
            var userWithRoles = await _unitOfWork.Users.GetWithRolesAsync(user.Id);
            userDtos.Add(MapToDto(userWithRoles ?? user));
        }

        return userDtos;
    }

    public async Task<UserDto?> GetUserByIdAsync(int id)
    {
        var user = await _unitOfWork.Users.GetWithRolesAsync(id);
        return user != null ? MapToDto(user) : null;
    }

    public async Task<UserDto> CreateUserAsync(CreateUserDto dto)
    {
        // Validate username
        if (string.IsNullOrWhiteSpace(dto.Username))
            throw new Exception("Tên đăng nhập không được để trống");

        var existingUser = await _unitOfWork.Users.GetByUsernameAsync(dto.Username);
        if (existingUser != null)
            throw new Exception("Tên đăng nhập đã tồn tại");

        // Validate email (required)
        if (string.IsNullOrWhiteSpace(dto.Email))
            throw new Exception("Email không được để trống (cần để gửi mật khẩu)");

        if (!IsValidEmail(dto.Email))
            throw new Exception("Email không đúng định dạng");

        var existingEmail = await _unitOfWork.Users.FindAsync(u => u.Email == dto.Email);
        if (existingEmail.Any())
            throw new Exception("Email đã được sử dụng");

        // Validate phone (if provided)
        if (!string.IsNullOrEmpty(dto.Phone))
        {
            if (!IsValidPhone(dto.Phone))
                throw new Exception("Số điện thoại phải có đúng 10 chữ số");

            var existingPhone = await _unitOfWork.Users.FindAsync(u => u.Phone == dto.Phone);
            if (existingPhone.Any())
                throw new Exception("Số điện thoại đã được sử dụng");
        }

        // Validate full name
        if (string.IsNullOrWhiteSpace(dto.FullName))
            throw new Exception("Họ tên không được để trống");

        // Generate random password
        var temporaryPassword = PasswordHelper.GenerateRandomPassword(8);

        var user = new User
        {
            Username = dto.Username.Trim(),
            PasswordHash = PasswordHelper.HashPassword(temporaryPassword),
            FullName = dto.FullName.Trim(),
            Email = dto.Email.Trim(),
            Phone = dto.Phone?.Trim(),
            IsActive = true,
            MustChangePassword = true, // Force change on first login
            CreatedAt = DateTime.Now
        };

        await _unitOfWork.Users.AddAsync(user);
        await _unitOfWork.SaveChangesAsync();

        // Assign role
        if (dto.RoleId > 0)
        {
            var userRole = new UserRole
            {
                UserId = user.Id,
                RoleId = dto.RoleId
            };
            await _unitOfWork.UserRoles.AddAsync(userRole);
            await _unitOfWork.SaveChangesAsync();
        }

        // Send welcome email with temporary password
        await _emailService.SendWelcomeEmailAsync(dto.Email, dto.FullName, dto.Username, temporaryPassword);

        var createdUser = await _unitOfWork.Users.GetWithRolesAsync(user.Id);
        return MapToDto(createdUser ?? user);
    }

    public async Task<UserDto> UpdateUserAsync(UserDto dto)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(dto.Id);
        if (user == null)
            throw new Exception("Không tìm thấy người dùng");

        // Validate email uniqueness if changed
        if (!string.IsNullOrEmpty(dto.Email) && dto.Email != user.Email)
        {
            if (!IsValidEmail(dto.Email))
                throw new Exception("Email không đúng định dạng");

            var existingEmail = await _unitOfWork.Users.FindAsync(u => u.Email == dto.Email && u.Id != dto.Id);
            if (existingEmail.Any())
                throw new Exception("Email đã được sử dụng");
        }

        // Validate phone uniqueness if changed
        if (!string.IsNullOrEmpty(dto.Phone) && dto.Phone != user.Phone)
        {
            if (!IsValidPhone(dto.Phone))
                throw new Exception("Số điện thoại phải có đúng 10 chữ số");

            var existingPhone = await _unitOfWork.Users.FindAsync(u => u.Phone == dto.Phone && u.Id != dto.Id);
            if (existingPhone.Any())
                throw new Exception("Số điện thoại đã được sử dụng");
        }

        user.FullName = dto.FullName;
        user.Email = dto.Email;
        user.Phone = dto.Phone;
        user.UpdatedAt = DateTime.Now;

        _unitOfWork.Users.Update(user);
        await _unitOfWork.SaveChangesAsync();

        return dto;
    }

    public async Task<bool> DeactivateUserAsync(int id)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(id);
        if (user == null) return false;

        user.IsActive = false;
        user.UpdatedAt = DateTime.Now;
        _unitOfWork.Users.Update(user);
        await _unitOfWork.SaveChangesAsync();
        
        return true;
    }

    public async Task<bool> ActivateUserAsync(int id)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(id);
        if (user == null) return false;

        user.IsActive = true;
        user.UpdatedAt = DateTime.Now;
        _unitOfWork.Users.Update(user);
        await _unitOfWork.SaveChangesAsync();
        
        return true;
    }

    public async Task<bool> ResetPasswordAsync(int userId, string newPassword)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(userId);
        if (user == null) return false;

        // Generate new password and send email
        var tempPassword = PasswordHelper.GenerateRandomPassword(8);
        
        user.PasswordHash = PasswordHelper.HashPassword(tempPassword);
        user.MustChangePassword = true;
        user.UpdatedAt = DateTime.Now;
        _unitOfWork.Users.Update(user);
        await _unitOfWork.SaveChangesAsync();

        // Send email
        if (!string.IsNullOrEmpty(user.Email))
        {
            await _emailService.SendPasswordResetEmailAsync(user.Email, user.FullName, tempPassword);
        }
        
        return true;
    }

    public async Task<IEnumerable<RoleDto>> GetAllRolesAsync()
    {
        var roles = await _unitOfWork.Roles.GetAllAsync();
        return roles.Select(r => new RoleDto
        {
            Id = r.Id,
            Name = r.Name,
            Description = r.Description
        });
    }

    public async Task<bool> AssignRoleAsync(int userId, int roleId)
    {
        var existing = await _unitOfWork.UserRoles.FindAsync(ur => ur.UserId == userId && ur.RoleId == roleId);
        if (existing.Any()) return true;

        var userRole = new UserRole
        {
            UserId = userId,
            RoleId = roleId
        };
        await _unitOfWork.UserRoles.AddAsync(userRole);
        await _unitOfWork.SaveChangesAsync();
        return true;
    }

    public async Task<bool> RemoveRoleAsync(int userId, int roleId)
    {
        var userRoles = await _unitOfWork.UserRoles.FindAsync(ur => ur.UserId == userId && ur.RoleId == roleId);
        var userRole = userRoles.FirstOrDefault();
        if (userRole == null) return false;

        _unitOfWork.UserRoles.Remove(userRole);
        await _unitOfWork.SaveChangesAsync();
        return true;
    }

    private static bool IsValidEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email)) return false;
        var emailPattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
        return Regex.IsMatch(email, emailPattern);
    }

    private static bool IsValidPhone(string phone)
    {
        if (string.IsNullOrWhiteSpace(phone)) return false;
        // Phone must be exactly 10 digits
        return Regex.IsMatch(phone, @"^\d{10}$");
    }

    private static UserDto MapToDto(User user) => new()
    {
        Id = user.Id,
        Username = user.Username,
        FullName = user.FullName,
        Email = user.Email,
        Phone = user.Phone,
        IsActive = user.IsActive,
        MustChangePassword = user.MustChangePassword,
        Roles = user.UserRoles?.Select(ur => ur.Role?.Name ?? "").Where(r => !string.IsNullOrEmpty(r)).ToList() ?? new List<string>()
    };
}

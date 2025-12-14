using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;

namespace FPTMart.BLL.Services;

public interface IEmailService
{
    Task<bool> SendEmailAsync(string toEmail, string subject, string body);
    Task<bool> SendWelcomeEmailAsync(string toEmail, string fullName, string username, string temporaryPassword);
    Task<bool> SendPasswordResetEmailAsync(string toEmail, string fullName, string newPassword);
}

public class EmailService : IEmailService
{
    private readonly string _smtpServer;
    private readonly int _smtpPort;
    private readonly string _senderEmail;
    private readonly string _senderName;
    private readonly string _password;

    public EmailService(IConfiguration configuration)
    {
        _smtpServer = configuration["SmtpSettings:Server"] ?? "smtp.gmail.com";
        _smtpPort = int.Parse(configuration["SmtpSettings:Port"] ?? "587");
        _senderEmail = configuration["SmtpSettings:SenderEmail"] ?? "";
        _senderName = configuration["SmtpSettings:SenderName"] ?? "FPTMart";
        _password = configuration["SmtpSettings:Password"] ?? "";
    }

    public async Task<bool> SendEmailAsync(string toEmail, string subject, string body)
    {
        try
        {
            if (string.IsNullOrEmpty(_senderEmail) || string.IsNullOrEmpty(_password))
            {
                // Email not configured, skip sending
                return false;
            }

            using var client = new SmtpClient(_smtpServer, _smtpPort)
            {
                Credentials = new NetworkCredential(_senderEmail, _password),
                EnableSsl = true
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress(_senderEmail, _senderName),
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            };
            mailMessage.To.Add(toEmail);

            await client.SendMailAsync(mailMessage);
            return true;
        }
        catch (Exception)
        {
            // Log error in production
            return false;
        }
    }

    public async Task<bool> SendWelcomeEmailAsync(string toEmail, string fullName, string username, string temporaryPassword)
    {
        var subject = "Chào mừng bạn đến với FPTMart";
        var body = $@"
            <html>
            <body style='font-family: Arial, sans-serif;'>
                <div style='max-width: 600px; margin: 0 auto; padding: 20px;'>
                    <div style='background-color: #1E3A5F; color: white; padding: 20px; text-align: center; border-radius: 10px 10px 0 0;'>
                        <h1>🛒 FPTMart</h1>
                    </div>
                    <div style='background-color: #f9f9f9; padding: 30px; border-radius: 0 0 10px 10px;'>
                        <h2>Xin chào {fullName},</h2>
                        <p>Tài khoản của bạn đã được tạo thành công trên hệ thống FPTMart.</p>
                        
                        <div style='background-color: #e8f5e9; padding: 20px; border-radius: 10px; margin: 20px 0;'>
                            <p><strong>Thông tin đăng nhập:</strong></p>
                            <p>👤 Username: <strong>{username}</strong></p>
                            <p>🔐 Mật khẩu tạm: <strong>{temporaryPassword}</strong></p>
                        </div>
                        
                        <p style='color: #d32f2f;'><strong>⚠️ Lưu ý:</strong> Vui lòng đổi mật khẩu ngay sau khi đăng nhập lần đầu.</p>
                        
                        <hr style='margin: 20px 0;'>
                        <p style='color: #666; font-size: 12px;'>Email này được gửi tự động từ hệ thống FPTMart. Vui lòng không reply.</p>
                    </div>
                </div>
            </body>
            </html>";

        return await SendEmailAsync(toEmail, subject, body);
    }

    public async Task<bool> SendPasswordResetEmailAsync(string toEmail, string fullName, string newPassword)
    {
        var subject = "Đặt lại mật khẩu - FPTMart";
        var body = $@"
            <html>
            <body style='font-family: Arial, sans-serif;'>
                <div style='max-width: 600px; margin: 0 auto; padding: 20px;'>
                    <div style='background-color: #1E3A5F; color: white; padding: 20px; text-align: center; border-radius: 10px 10px 0 0;'>
                        <h1>🔐 Đặt Lại Mật Khẩu</h1>
                    </div>
                    <div style='background-color: #f9f9f9; padding: 30px; border-radius: 0 0 10px 10px;'>
                        <h2>Xin chào {fullName},</h2>
                        <p>Mật khẩu của bạn đã được đặt lại.</p>
                        
                        <div style='background-color: #fff3e0; padding: 20px; border-radius: 10px; margin: 20px 0;'>
                            <p>🔐 Mật khẩu mới: <strong>{newPassword}</strong></p>
                        </div>
                        
                        <p style='color: #d32f2f;'><strong>⚠️ Lưu ý:</strong> Vui lòng đổi mật khẩu ngay sau khi đăng nhập.</p>
                        
                        <hr style='margin: 20px 0;'>
                        <p style='color: #666; font-size: 12px;'>Nếu bạn không yêu cầu đặt lại mật khẩu, vui lòng liên hệ admin ngay.</p>
                    </div>
                </div>
            </body>
            </html>";

        return await SendEmailAsync(toEmail, subject, body);
    }
}

public static class PasswordHelper
{
    private const string ValidChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890!@#$%";

    public static string GenerateRandomPassword(int length = 8)
    {
        var random = new Random();
        var password = new char[length];
        for (int i = 0; i < length; i++)
        {
            password[i] = ValidChars[random.Next(ValidChars.Length)];
        }
        return new string(password);
    }

    public static string HashPassword(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password);
    }

    public static bool VerifyPassword(string password, string hashedPassword)
    {
        try
        {
            return BCrypt.Net.BCrypt.Verify(password, hashedPassword);
        }
        catch
        {
            // For migration: if hash fails, try plain text comparison
            return password == hashedPassword;
        }
    }
}

using API.Configuration;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Mail;

namespace API.Services.Email
{
    public class EmailService : IEmailService
    {
        private readonly EmailOptions _emailOptions;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IOptions<EmailOptions> emailOptions, ILogger<EmailService> logger)
        {
            _emailOptions = emailOptions.Value;
            _logger = logger;
        }

        public async Task<bool> SendOtpEmailAsync(string toEmail, string otpCode, string verificationUrl)
        {
            SmtpClient client = null;
            MailMessage mailMessage = null;

            try
            {
                // Create SMTP client with proper configuration
                client = new SmtpClient(_emailOptions.SmtpHost, _emailOptions.SmtpPort)
                {
                    Credentials = new NetworkCredential(_emailOptions.Username, _emailOptions.Password),
                    EnableSsl = _emailOptions.EnableSsl,
                    Timeout = 10000 // 10 seconds timeout
                };

                mailMessage = new MailMessage
                {
                    From = new MailAddress(_emailOptions.SenderEmail, _emailOptions.SenderName),
                    Subject = "Email Verification - Asset Management System",
                    Body = GetEmailBody(otpCode, verificationUrl),
                    IsBodyHtml = true
                };

                mailMessage.To.Add(toEmail);

                // Send the email asynchronously
                await client.SendMailAsync(mailMessage);

                _logger.LogInformation("OTP email sent successfully to {Email}", toEmail);
                return true;
            }
            catch (SmtpException smtpEx)
            {
                _logger.LogError(smtpEx, "SMTP error occurred while sending OTP email to {Email}. Error: {Message}",
                    toEmail, smtpEx.Message);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send OTP email to {Email}", toEmail);
                return false;
            }
            finally
            {
                // Properly dispose of resources
                mailMessage?.Dispose();
                client?.Dispose();
            }
        }

        private string GetEmailBody(string otpCode, string verificationUrl)
        {
            return $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background-color: #4CAF50; color: white; padding: 20px; text-align: center; }}
        .content {{ background-color: #f9f9f9; padding: 30px; border-radius: 5px; margin-top: 20px; }}
        .otp-code {{ font-size: 32px; font-weight: bold; color: #4CAF50; text-align: center;
                     padding: 20px; background-color: #fff; border: 2px dashed #4CAF50;
                     border-radius: 5px; margin: 20px 0; letter-spacing: 5px; }}
        .button {{ display: inline-block; padding: 12px 30px; background-color: #4CAF50;
                   color: white; text-decoration: none; border-radius: 5px; margin-top: 20px; }}
        .footer {{ text-align: center; margin-top: 30px; font-size: 12px; color: #666; }}
        .warning {{ color: #d32f2f; font-size: 14px; margin-top: 20px; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>Email Verification</h1>
        </div>
        <div class='content'>
            <h2>Welcome to Asset Management System!</h2>
            <p>Thank you for registering. Please verify your email address to complete your registration.</p>

            <p>Your One-Time Password (OTP) is:</p>
            <div class='otp-code'>{otpCode}</div>

            <p>Or click the button below to verify your email:</p>
            <div style='text-align: center;'>
                <a href='{verificationUrl}' class='button'>Verify Email</a>
            </div>

            <p class='warning'>
                ⚠️ This OTP will expire in 10 minutes.<br>
                ⚠️ Do not share this code with anyone.
            </p>
        </div>
        <div class='footer'>
            <p>If you didn't request this verification, please ignore this email.</p>
            <p>&copy; 2024 Asset Management System. All rights reserved.</p>
        </div>
    </div>
</body>
</html>";
        }
    }
}
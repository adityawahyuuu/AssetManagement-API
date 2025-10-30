namespace API.Services.Email
{
    public interface IEmailService
    {
        Task<bool> SendOtpEmailAsync(string toEmail, string otpCode, string verificationUrl);
    }
}

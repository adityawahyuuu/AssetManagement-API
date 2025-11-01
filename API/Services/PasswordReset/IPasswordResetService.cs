using API.Common;

namespace API.Services.PasswordReset
{
    public interface IPasswordResetService
    {
        Task<Result<string>> GenerateResetTokenAsync(string email);
        Task<Result> VerifyResetTokenAsync(string email, string token);
        Task<Result> CleanupExpiredTokensAsync(string email);
    }
}

using API.Common;
using API.Configuration;
using API.Constants;
using API.Data;
using API.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Security.Cryptography;

namespace API.Services.PasswordReset
{
    public class PasswordResetService : IPasswordResetService
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly PasswordResetOptions _options;
        private readonly ILogger<PasswordResetService> _logger;

        public PasswordResetService(
            ApplicationDbContext dbContext,
            IOptions<PasswordResetOptions> options,
            ILogger<PasswordResetService> logger)
        {
            _dbContext = dbContext;
            _options = options.Value;
            _logger = logger;
        }

        public async Task<Result<string>> GenerateResetTokenAsync(string email)
        {
            try
            {
                // Check if user exists and is confirmed
                var user = await _dbContext.user_logins
                    .FirstOrDefaultAsync(u => u.email == email);

                if (user == null)
                {
                    // For security: Don't reveal if email exists
                    _logger.LogWarning("Password reset token requested for non-existent email: {Email}", email);
                    return Result.Failure<string>(ResponseMessages.UserNotFound);
                }

                // Check if user is confirmed
                var isConfirmed = user.is_confirmed != null && user.is_confirmed[0];
                if (!isConfirmed)
                {
                    _logger.LogWarning("Password reset token requested for unconfirmed user: {Email}", email);
                    return Result.Failure<string>("User account is not confirmed");
                }

                // Delete any existing tokens for this email
                var existingTokens = await _dbContext.password_reset_tokens
                    .Where(t => t.email == email)
                    .ToListAsync();

                if (existingTokens.Any())
                {
                    _dbContext.password_reset_tokens.RemoveRange(existingTokens);
                    await _dbContext.SaveChangesAsync();
                }

                // Generate secure random token
                var token = GenerateSecureToken();
                var expiresAt = DateTime.Now.AddMinutes(_options.TokenExpirationMinutes);

                var resetToken = new password_reset_tokens
                {
                    email = email,
                    token = token,
                    created_at = DateTime.Now,
                    expires_at = expiresAt,
                    is_used = false
                };

                await _dbContext.password_reset_tokens.AddAsync(resetToken);
                await _dbContext.SaveChangesAsync();

                _logger.LogInformation("Password reset token generated for email: {Email}", email);
                return Result.Success(token);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to generate password reset token for email: {Email}", email);
                return Result.Failure<string>("Failed to generate password reset token");
            }
        }

        public async Task<Result> VerifyResetTokenAsync(string email, string token)
        {
            try
            {
                // Find the token record
                var tokenRecord = await _dbContext.password_reset_tokens
                    .FirstOrDefaultAsync(t => t.email == email && t.token == token && !t.is_used);

                if (tokenRecord == null)
                {
                    return Result.Failure("Invalid or already used password reset token");
                }

                // Check if token expired
                if (tokenRecord.expires_at < DateTime.Now)
                {
                    return Result.Failure("Password reset token has expired");
                }

                // Mark token as used
                tokenRecord.is_used = true;
                await _dbContext.SaveChangesAsync();

                _logger.LogInformation("Password reset token verified successfully for email: {Email}", email);
                return Result.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to verify password reset token for email: {Email}", email);
                return Result.Failure("Failed to verify password reset token");
            }
        }

        public async Task<Result> CleanupExpiredTokensAsync(string email)
        {
            try
            {
                var expiredTokens = await _dbContext.password_reset_tokens
                    .Where(t => t.email == email && (t.expires_at < DateTime.Now || t.is_used))
                    .ToListAsync();

                if (expiredTokens.Any())
                {
                    _dbContext.password_reset_tokens.RemoveRange(expiredTokens);
                    await _dbContext.SaveChangesAsync();
                    _logger.LogInformation("Cleaned up {Count} expired/used tokens for email: {Email}",
                        expiredTokens.Count, email);
                }

                return Result.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to cleanup expired tokens for email: {Email}", email);
                return Result.Failure("Failed to cleanup expired tokens");
            }
        }

        private string GenerateSecureToken()
        {
            var random = new Random();
            var code = random.Next(0, (int)Math.Pow(10, _options.CodeLength))
                             .ToString($"D{_options.CodeLength}");
            return code;
        }
    }
}

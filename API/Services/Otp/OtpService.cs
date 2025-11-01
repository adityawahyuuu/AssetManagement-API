using API.Common;
using API.Configuration;
using API.Constants;
using API.Data;
using API.DTOs;
using API.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace API.Services.Otp
{
    public class OtpService : IOtpService
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly OtpOptions _otpOptions;
        private readonly ILogger<OtpService> _logger;

        public OtpService(
            ApplicationDbContext dbContext,
            IOptions<OtpOptions> otpOptions,
            ILogger<OtpService> logger)
        {
            _dbContext = dbContext;
            _otpOptions = otpOptions.Value;
            _logger = logger;
        }

        public async Task<Result<string>> GenerateAndSaveOtpAsync(string email)
        {
            try
            {
                // Check if pending user exists (required for FK constraint)
                var pendingUser = await _dbContext.pending_users
                    .FirstOrDefaultAsync(p => p.email == email);

                if (pendingUser == null)
                {
                    return Result.Failure<string>(OtpMessages.PendingUserNotFound);
                }

                // Check if pending registration expired
                if (pendingUser.expires_at < DateTime.Now)
                {
                    return Result.Failure<string>(OtpMessages.RegistrationExpired);
                }

                // Delete any existing OTP for this email
                var existingOtps = await _dbContext.otp_codes
                    .Where(o => o.email == email)
                    .ToListAsync();

                if (existingOtps.Any())
                {
                    _dbContext.otp_codes.RemoveRange(existingOtps);
                    await _dbContext.SaveChangesAsync();
                }

                // Generate new OTP
                var otpCode = GenerateOtpCode();
                var expiresAt = DateTime.Now.AddMinutes(_otpOptions.ExpirationMinutes);

                var otp = new otp_codes
                {
                    email = email,
                    otp_code = otpCode,
                    created_at = DateTime.Now,
                    expires_at = expiresAt,
                    is_verified = false,
                    attempts = 0,
                    max_attempts = _otpOptions.MaxAttempts
                };

                await _dbContext.otp_codes.AddAsync(otp);
                await _dbContext.SaveChangesAsync();

                _logger.LogInformation("OTP generated for email: {Email}", email);
                return Result.Success(otpCode);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to generate OTP for email: {Email}", email);
                return Result.Failure<string>(OtpMessages.FailedToGenerateOtp);
            }
        }

        public async Task<Result> VerifyOtpAsync(VerifyOtpDto verifyOtpDto)
        {
            try
            {
                // Find OTP record
                var otpRecord = await _dbContext.otp_codes
                    .FirstOrDefaultAsync(o => o.email == verifyOtpDto.Email && !o.is_verified);

                if (otpRecord == null)
                {
                    return Result.Failure(OtpMessages.OtpNotFound);
                }

                // Check if OTP expired
                if (otpRecord.expires_at < DateTime.Now)
                {
                    return Result.Failure(OtpMessages.OtpExpired);
                }

                // Check max attempts
                if (otpRecord.attempts >= otpRecord.max_attempts)
                {
                    return Result.Failure(OtpMessages.OtpMaxAttemptsReached);
                }

                // Verify OTP code
                if (otpRecord.otp_code != verifyOtpDto.OtpCode)
                {
                    otpRecord.attempts++;
                    await _dbContext.SaveChangesAsync();

                    var remainingAttempts = otpRecord.max_attempts - otpRecord.attempts;
                    return Result.Failure($"{OtpMessages.OtpInvalid} Remaining attempts: {remainingAttempts}");
                }

                // Mark as verified
                otpRecord.is_verified = true;
                await _dbContext.SaveChangesAsync();

                _logger.LogInformation("OTP verified successfully for email: {Email}", verifyOtpDto.Email);
                return Result.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to verify OTP for email: {Email}", verifyOtpDto.Email);
                return Result.Failure(OtpMessages.FailedToVerifyOtp);
            }
        }

        public async Task<Result> ResendOtpAsync(string email)
        {
            try
            {
                // Check if pending user exists
                var pendingUser = await _dbContext.pending_users
                    .FirstOrDefaultAsync(p => p.email == email);

                if (pendingUser == null)
                {
                    return Result.Failure(OtpMessages.PendingUserNotFound);
                }

                // Check if pending registration expired
                if (pendingUser.expires_at < DateTime.Now)
                {
                    return Result.Failure(OtpMessages.RegistrationExpired);
                }

                // Generate new OTP
                var result = await GenerateAndSaveOtpAsync(email);
                if (result.IsFailure)
                {
                    return Result.Failure(result.Error);
                }

                _logger.LogInformation("OTP resent for email: {Email}", email);
                return Result.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to resend OTP for email: {Email}", email);
                return Result.Failure(OtpMessages.FailedToResendOtp);
            }
        }

        private string GenerateOtpCode()
        {
            var random = new Random();
            var code = random.Next(0, (int)Math.Pow(10, _otpOptions.CodeLength))
                             .ToString($"D{_otpOptions.CodeLength}");
            return code;
        }
    }
}

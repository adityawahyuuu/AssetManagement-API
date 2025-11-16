using API.Common;
using API.Configuration;
using API.Constants;
using API.Data;
using API.DTOs;
using API.Models;
using API.Services.Email;
using API.Services.Jwt;
using API.Services.Otp;
using API.Services.PasswordHashing;
using API.Services.PasswordReset;
using AutoMapper;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Collections;

namespace API.Repositories.Register
{
    public class RegisterRepository : IRegisterRepository
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly IValidator<UserRegisterDto> _validator;
        private readonly IValidator<ResetPasswordDto> _resetPasswordValidator;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IMapper _mapper;
        private readonly IOtpService _otpService;
        private readonly IPasswordResetService _passwordResetService;
        private readonly IEmailService _emailService;
        private readonly IJwtService _jwtService;
        private readonly OtpOptions _otpOptions;
        private readonly ILogger<RegisterRepository> _logger;

        public RegisterRepository(
            ApplicationDbContext context,
            IValidator<UserRegisterDto> validator,
            IValidator<ResetPasswordDto> resetPasswordValidator,
            IPasswordHasher passwordHasher,
            IMapper mapper,
            IOtpService otpService,
            IPasswordResetService passwordResetService,
            IEmailService emailService,
            IJwtService jwtService,
            IOptions<OtpOptions> otpOptions,
            ILogger<RegisterRepository> logger)
        {
            _dbContext = context;
            _validator = validator;
            _resetPasswordValidator = resetPasswordValidator;
            _passwordHasher = passwordHasher;
            _mapper = mapper;
            _otpService = otpService;
            _passwordResetService = passwordResetService;
            _emailService = emailService;
            _jwtService = jwtService;
            _otpOptions = otpOptions.Value;
            _logger = logger;
        }

        public async Task<Result<RegistrationResponseDto>> CreatePendingUser(UserRegisterDto userRegister, string baseUrl)
        {
            // Validation
            var validationResult = await _validator.ValidateAsync(userRegister);
            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
                return Result.Failure<RegistrationResponseDto>(errors);
            }

            // Check if email already exists in user_logins
            var existingUser = await _dbContext.user_logins
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.email == userRegister.Email);

            if (existingUser != null)
            {
                return Result.Failure<RegistrationResponseDto>(ResponseMessages.EmailAlreadyRegistered);
            }

            // Check if email already exists in pending_users
            var existingPendingUser = await _dbContext.pending_users
                .AsTracking()
                .FirstOrDefaultAsync(u => u.email == userRegister.Email);

            try
            {
                // Check if we're using in-memory database (transactions not supported)
                var isInMemory = _dbContext.Database.ProviderName == "Microsoft.EntityFrameworkCore.InMemory";

                string otpCode;

                if (isInMemory)
                {
                    // In-memory database: no transaction support
                    if (existingPendingUser != null)
                    {
                        // Delete existing pending user and try again
                        _dbContext.pending_users.Remove(existingPendingUser);
                        await _dbContext.SaveChangesAsync();
                    }

                    // Create pending user
                    var pendingUser = new pending_users
                    {
                        email = userRegister.Email,
                        username = userRegister.Username,
                        password_hash = _passwordHasher.HashPassword(userRegister.Password),
                        created_at = DateTime.Now,
                        expires_at = DateTime.Now.AddMinutes(_otpOptions.PendingUserExpirationMinutes)
                    };

                    await _dbContext.pending_users.AddAsync(pendingUser);
                    await _dbContext.SaveChangesAsync();

                    // Generate OTP
                    var otpResult = await _otpService.GenerateAndSaveOtpAsync(userRegister.Email);
                    if (otpResult.IsFailure)
                    {
                        return Result.Failure<RegistrationResponseDto>(otpResult.Error);
                    }
                    otpCode = otpResult.Value;
                }
                else
                {
                    // Real database: use transaction with execution strategy for retry support
                    var strategy = _dbContext.Database.CreateExecutionStrategy();
                    otpCode = await strategy.ExecuteAsync(async () =>
                    {
                        using var transaction = await _dbContext.Database.BeginTransactionAsync();
                        try
                        {
                            if (existingPendingUser != null)
                            {
                                // Delete existing pending user and try again
                                _dbContext.pending_users.Remove(existingPendingUser);
                                await _dbContext.SaveChangesAsync();
                            }

                            // Create pending user
                            var pendingUser = new pending_users
                            {
                                email = userRegister.Email,
                                username = userRegister.Username,
                                password_hash = _passwordHasher.HashPassword(userRegister.Password),
                                created_at = DateTime.Now,
                                expires_at = DateTime.Now.AddMinutes(_otpOptions.PendingUserExpirationMinutes)
                            };

                            await _dbContext.pending_users.AddAsync(pendingUser);
                            await _dbContext.SaveChangesAsync();

                            // Generate OTP
                            var otpResult = await _otpService.GenerateAndSaveOtpAsync(userRegister.Email);
                            if (otpResult.IsFailure)
                            {
                                throw new InvalidOperationException(otpResult.Error);
                            }

                            await transaction.CommitAsync();
                            return otpResult.Value;
                        }
                        catch (Exception ex)
                        {
                            await transaction.RollbackAsync();
                            throw;
                        }
                    });
                }

                // Send OTP email
                var verificationUrl = $"{baseUrl}/user/verify?email={userRegister.Email}&otp={otpCode}";
                var emailSent = await _emailService.SendOtpEmailAsync(
                    userRegister.Email,
                    otpCode,
                    verificationUrl);

                if (!emailSent)
                {
                    _logger.LogWarning("Failed to send OTP email to {Email}", userRegister.Email);
                    return Result.Failure<RegistrationResponseDto>(OtpMessages.FailedToSendOtpEmail);
                }

                var response = new RegistrationResponseDto
                {
                    Email = userRegister.Email,
                    Message = OtpMessages.OtpSentSuccessfully,
                    ExpirationMinutes = _otpOptions.ExpirationMinutes
                };

                return Result.Success(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create pending user for {Email}", userRegister.Email);
                return Result.Failure<RegistrationResponseDto>(ResponseMessages.FailedToCreateUser);
            }
        }

        public async Task<Result> ActivateUser(string email)
        {
            try
            {
                // Get pending user
                var pendingUser = await _dbContext.pending_users
                    .AsTracking()
                    .FirstOrDefaultAsync(p => p.email == email);

                if (pendingUser == null)
                {
                    return Result.Failure(OtpMessages.PendingUserDataNotFound);
                }

                // Check if registration expired
                if (pendingUser.expires_at < DateTime.Now)
                {
                    _dbContext.pending_users.Remove(pendingUser);
                    await _dbContext.SaveChangesAsync();
                    return Result.Failure(OtpMessages.RegistrationExpired);
                }

                // Check if we're using in-memory database (transactions not supported)
                var isInMemory = _dbContext.Database.ProviderName == "Microsoft.EntityFrameworkCore.InMemory";

                if (isInMemory)
                {
                    // In-memory database: no transaction support
                    var newUser = new user_login
                    {
                        email = pendingUser.email,
                        username = pendingUser.username,
                        password_hash = pendingUser.password_hash,
                        created_at = DateTime.Now,
                        updated_at = DateTime.Now,
                        is_confirmed = new BitArray(1, true)
                    };

                    await _dbContext.user_logins.AddAsync(newUser);
                    _dbContext.pending_users.Remove(pendingUser);
                    await _dbContext.SaveChangesAsync();
                }
                else
                {
                    // Real database: use transaction with execution strategy for retry support
                    var strategy = _dbContext.Database.CreateExecutionStrategy();
                    await strategy.ExecuteAsync(async () =>
                    {
                        using var transaction = await _dbContext.Database.BeginTransactionAsync();
                        try
                        {
                            // Create actual user account
                            var newUser = new user_login
                            {
                                email = pendingUser.email,
                                username = pendingUser.username,
                                password_hash = pendingUser.password_hash,
                                created_at = DateTime.Now,
                                updated_at = DateTime.Now,
                                is_confirmed = new BitArray(1, true)
                            };

                            await _dbContext.user_logins.AddAsync(newUser);

                            // Remove pending user (OTP records will be CASCADE deleted)
                            _dbContext.pending_users.Remove(pendingUser);

                            await _dbContext.SaveChangesAsync();
                            await transaction.CommitAsync();
                        }
                        catch
                        {
                            await transaction.RollbackAsync();
                            throw;
                        }
                    });
                }

                _logger.LogInformation("User activated successfully: {Email}", email);
                return Result.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to activate user: {Email}", email);
                return Result.Failure(OtpMessages.FailedToActivateUser);
            }
        }

        public async Task<Result<LoginResponseDto>> Login(LoginDto loginDto)
        {
            try
            {
                // Find user by email
                var user = await _dbContext.user_logins
                    .AsNoTracking()
                    .FirstOrDefaultAsync(u => u.email == loginDto.Email);

                if (user == null)
                {
                    // Don't reveal if email exists
                    return Result.Failure<LoginResponseDto>(ResponseMessages.InvalidCredentials);
                }

                // Check if account is confirmed
                var isConfirmed = user.is_confirmed != null && user.is_confirmed[0];
                if (!isConfirmed)
                {
                    return Result.Failure<LoginResponseDto>(ResponseMessages.AccountNotConfirmed);
                }

                // Verify password
                var isPasswordValid = _passwordHasher.VerifyPassword(loginDto.Password, user.password_hash);
                if (!isPasswordValid)
                {
                    return Result.Failure<LoginResponseDto>(ResponseMessages.InvalidCredentials);
                }

                // Generate JWT token
                var token = _jwtService.GenerateToken(user);

                // Create response
                var response = new LoginResponseDto
                {
                    UserId = user.userid,
                    Email = user.email,
                    Username = user.username,
                    CreatedAt = user.created_at,
                    Token = token
                };

                _logger.LogInformation("User logged in successfully: {Email}", loginDto.Email);
                return Result.Success(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to login user: {Email}", loginDto.Email);
                return Result.Failure<LoginResponseDto>("Failed to login. Please try again later.");
            }
        }

        public async Task<Result<AuthMeResponseDto>> GetCurrentUser(int userId)
        {
            try
            {
                // Find user by ID
                var user = await _dbContext.user_logins
                    .AsNoTracking()
                    .FirstOrDefaultAsync(u => u.userid == userId);

                if (user == null)
                {
                    return Result.Failure<AuthMeResponseDto>(ResponseMessages.UserNotFound);
                }

                // Check if account is confirmed
                var isConfirmed = user.is_confirmed != null && user.is_confirmed[0];

                // Create response
                var response = new AuthMeResponseDto
                {
                    UserId = user.userid,
                    Email = user.email,
                    Username = user.username,
                    CreatedAt = user.created_at,
                    IsConfirmed = isConfirmed
                };

                return Result.Success(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get current user: {UserId}", userId);
                return Result.Failure<AuthMeResponseDto>("Failed to retrieve user information.");
            }
        }

        public async Task<Result> SendPasswordResetOtp(string email)
        {
            try
            {
                // Generate password reset token
                var tokenResult = await _passwordResetService.GenerateResetTokenAsync(email);
                if (tokenResult.IsFailure)
                {
                    // For security: Don't reveal specific error to client
                    // Log the actual error but return generic message
                    _logger.LogWarning("Password reset token generation failed for {Email}: {Error}",
                        email, tokenResult.Error);
                    return Result.Success(); // Don't reveal if email exists
                }

                // Send password reset email with token
                var emailSent = await _emailService.SendPasswordResetEmailAsync(
                    email,
                    tokenResult.Value);

                if (!emailSent)
                {
                    _logger.LogWarning("Failed to send password reset email to {Email}", email);
                    return Result.Failure("Failed to send password reset email");
                }

                _logger.LogInformation("Password reset token sent to: {Email}", email);
                return Result.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending password reset token for {Email}", email);
                return Result.Failure(ResponseMessages.FailedToResetPassword);
            }
        }

        public async Task<Result> ResetPassword(ResetPasswordDto resetPasswordDto)
        {
            try
            {
                // Validation
                var validationResult = await _resetPasswordValidator.ValidateAsync(resetPasswordDto);
                if (!validationResult.IsValid)
                {
                    var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
                    return Result.Failure(errors);
                }

                // Check if we're using in-memory database (transactions not supported)
                var isInMemory = _dbContext.Database.ProviderName == "Microsoft.EntityFrameworkCore.InMemory";

                if (isInMemory)
                {
                    // In-memory database: no transaction support
                    // Verify reset token (marks as used in database)
                    var tokenVerifyResult = await _passwordResetService.VerifyResetTokenAsync(
                        resetPasswordDto.Email,
                        resetPasswordDto.OtpCode); // Using OtpCode field for token (backwards compatibility)

                    if (tokenVerifyResult.IsFailure)
                    {
                        return Result.Failure(tokenVerifyResult.Error);
                    }

                    // Get user and update password
                    var user = await _dbContext.user_logins
                        .AsTracking()
                        .FirstOrDefaultAsync(u => u.email == resetPasswordDto.Email);

                    if (user == null)
                    {
                        return Result.Failure(ResponseMessages.UserNotFound);
                    }

                    // Update password
                    user.password_hash = _passwordHasher.HashPassword(resetPasswordDto.Password);
                    user.updated_at = DateTime.Now;

                    await _dbContext.SaveChangesAsync();

                    // Clean up password reset tokens
                    await _passwordResetService.CleanupExpiredTokensAsync(resetPasswordDto.Email);
                }
                else
                {
                    // Real database: use transaction with execution strategy for retry support
                    var strategy = _dbContext.Database.CreateExecutionStrategy();
                    await strategy.ExecuteAsync(async () =>
                    {
                        using var transaction = await _dbContext.Database.BeginTransactionAsync();
                        try
                        {
                            // Verify reset token (marks as used in database)
                            var tokenVerifyResult = await _passwordResetService.VerifyResetTokenAsync(
                                resetPasswordDto.Email,
                                resetPasswordDto.OtpCode); // Using OtpCode field for token (backwards compatibility)

                            if (tokenVerifyResult.IsFailure)
                            {
                                throw new InvalidOperationException(tokenVerifyResult.Error);
                            }

                            // Get user and update password
                            var user = await _dbContext.user_logins
                                .AsTracking()
                                .FirstOrDefaultAsync(u => u.email == resetPasswordDto.Email);

                            if (user == null)
                            {
                                throw new InvalidOperationException(ResponseMessages.UserNotFound);
                            }

                            // Update password
                            user.password_hash = _passwordHasher.HashPassword(resetPasswordDto.Password);
                            user.updated_at = DateTime.Now;

                            await _dbContext.SaveChangesAsync();

                            // Clean up password reset tokens
                            await _passwordResetService.CleanupExpiredTokensAsync(resetPasswordDto.Email);

                            await transaction.CommitAsync();
                        }
                        catch
                        {
                            await transaction.RollbackAsync();
                            throw;
                        }
                    });
                }

                _logger.LogInformation("Password reset successfully for: {Email}", resetPasswordDto.Email);
                return Result.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to reset password for {Email}", resetPasswordDto.Email);
                return Result.Failure(ResponseMessages.FailedToResetPassword);
            }
        }
    }
}

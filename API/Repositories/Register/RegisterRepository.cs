using API.Common;
using API.Configuration;
using API.Constants;
using API.Data;
using API.DTOs;
using API.Models;
using API.Services.Email;
using API.Services.Otp;
using API.Services.PasswordHashing;
using AutoMapper;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace API.Repositories.Register
{
    public class RegisterRepository : IRegisterRepository
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly IValidator<UserRegisterDto> _validator;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IMapper _mapper;
        private readonly IOtpService _otpService;
        private readonly IEmailService _emailService;
        private readonly OtpOptions _otpOptions;
        private readonly ILogger<RegisterRepository> _logger;

        public RegisterRepository(
            ApplicationDbContext context,
            IValidator<UserRegisterDto> validator,
            IPasswordHasher passwordHasher,
            IMapper mapper,
            IOtpService otpService,
            IEmailService emailService,
            IOptions<OtpOptions> otpOptions,
            ILogger<RegisterRepository> logger)
        {
            _dbContext = context;
            _validator = validator;
            _passwordHasher = passwordHasher;
            _mapper = mapper;
            _otpService = otpService;
            _emailService = emailService;
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
                .FirstOrDefaultAsync(u => u.email == userRegister.Email);

            if (existingUser != null)
            {
                return Result.Failure<RegistrationResponseDto>(ResponseMessages.EmailAlreadyRegistered);
            }

            // Check if email already exists in pending_users
            var existingPendingUser = await _dbContext.pending_users
                .FirstOrDefaultAsync(u => u.email == userRegister.Email);

            if (existingPendingUser != null)
            {
                // Delete existing pending user and try again
                _dbContext.pending_users.Remove(existingPendingUser);
                await _dbContext.SaveChangesAsync();
            }

            try
            {
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

                // Send OTP email
                var verificationUrl = $"{baseUrl}/user/verify?email={userRegister.Email}&otp={otpResult.Value}";
                var emailSent = await _emailService.SendOtpEmailAsync(
                    userRegister.Email,
                    otpResult.Value,
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

                // Create actual user account
                var newUser = new user_login
                {
                    email = pendingUser.email,
                    username = pendingUser.username,
                    password_hash = pendingUser.password_hash,
                    created_at = DateTime.Now,
                    updated_at = DateTime.Now,
                    is_confirmed = true
                };

                await _dbContext.user_logins.AddAsync(newUser);

                // Remove pending user and OTP records
                _dbContext.pending_users.Remove(pendingUser);

                var otpRecords = await _dbContext.otp_codes
                    .Where(o => o.email == email)
                    .ToListAsync();
                _dbContext.otp_codes.RemoveRange(otpRecords);

                await _dbContext.SaveChangesAsync();

                _logger.LogInformation("User activated successfully: {Email}", email);
                return Result.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to activate user: {Email}", email);
                return Result.Failure(OtpMessages.FailedToActivateUser);
            }
        }
    }
}

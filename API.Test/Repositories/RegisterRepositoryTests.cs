using API.Common;
using API.Configuration;
using API.Constants;
using API.Data;
using API.DTOs;
using API.Repositories.Register;
using API.Services.Email;
using API.Services.Jwt;
using API.Services.Otp;
using API.Services.PasswordHashing;
using API.Services.PasswordReset;
using API.Test.Helpers;
using AutoMapper;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace API.Test.Repositories
{
    public class RegisterRepositoryTests : IDisposable
    {
        private readonly RegisterRepository _repository;
        private readonly ApplicationDbContext _dbContext;
        private readonly Mock<IValidator<UserRegisterDto>> _validatorMock;
        private readonly Mock<IValidator<ResetPasswordDto>> _resetPasswordValidatorMock;
        private readonly Mock<IPasswordHasher> _passwordHasherMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly Mock<IOtpService> _otpServiceMock;
        private readonly Mock<IPasswordResetService> _passwordResetServiceMock;
        private readonly Mock<IEmailService> _emailServiceMock;
        private readonly Mock<IJwtService> _jwtServiceMock;
        private readonly Mock<ILogger<RegisterRepository>> _loggerMock;

        public RegisterRepositoryTests()
        {
            _dbContext = TestDbContextFactory.CreateInMemoryContext(Guid.NewGuid().ToString());
            _validatorMock = new Mock<IValidator<UserRegisterDto>>();
            _resetPasswordValidatorMock = new Mock<IValidator<ResetPasswordDto>>();
            _passwordHasherMock = new Mock<IPasswordHasher>();
            _mapperMock = new Mock<IMapper>();
            _otpServiceMock = new Mock<IOtpService>();
            _passwordResetServiceMock = new Mock<IPasswordResetService>();
            _emailServiceMock = new Mock<IEmailService>();
            _jwtServiceMock = new Mock<IJwtService>();
            _loggerMock = new Mock<ILogger<RegisterRepository>>();

            var otpOptions = Options.Create(new OtpOptions
            {
                CodeLength = 6,
                ExpirationMinutes = 10,
                MaxAttempts = 5,
                PendingUserExpirationMinutes = 30
            });

            _repository = new RegisterRepository(
                _dbContext,
                _validatorMock.Object,
                _resetPasswordValidatorMock.Object,
                _passwordHasherMock.Object,
                _mapperMock.Object,
                _otpServiceMock.Object,
                _passwordResetServiceMock.Object,
                _emailServiceMock.Object,
                _jwtServiceMock.Object,
                otpOptions,
                _loggerMock.Object
            );
        }

        [Fact]
        public async Task CreatePendingUser_WithValidData_ShouldSucceed()
        {
            // Arrange
            var userDto = TestDataBuilder.CreateValidUserRegisterDto();
            var baseUrl = "http://localhost";

            _validatorMock.Setup(v => v.ValidateAsync(It.IsAny<UserRegisterDto>(), default))
                .ReturnsAsync(new ValidationResult());

            _passwordHasherMock.Setup(p => p.HashPassword(It.IsAny<string>()))
                .Returns("hashedpassword");

            _otpServiceMock.Setup(o => o.GenerateAndSaveOtpAsync(It.IsAny<string>()))
                .ReturnsAsync(Result.Success("123456"));

            _emailServiceMock.Setup(e => e.SendOtpEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(true);

            // Act
            var result = await _repository.CreatePendingUser(userDto, baseUrl);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Email.Should().Be(userDto.Email);
            result.Value.ExpirationMinutes.Should().Be(10);

            var pendingUser = await _dbContext.pending_users.FirstOrDefaultAsync(p => p.email == userDto.Email);
            pendingUser.Should().NotBeNull();
            pendingUser!.username.Should().Be(userDto.Username);
        }

        [Fact]
        public async Task CreatePendingUser_WithInvalidData_ShouldFail()
        {
            // Arrange
            var userDto = TestDataBuilder.CreateValidUserRegisterDto();
            var baseUrl = "http://localhost";

            var validationFailures = new List<ValidationFailure>
            {
                new ValidationFailure("Email", "Email is required")
            };

            _validatorMock.Setup(v => v.ValidateAsync(It.IsAny<UserRegisterDto>(), default))
                .ReturnsAsync(new ValidationResult(validationFailures));

            // Act
            var result = await _repository.CreatePendingUser(userDto, baseUrl);

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Errors.Should().Contain("Email is required");
        }

        [Fact]
        public async Task CreatePendingUser_WithExistingEmail_ShouldFail()
        {
            // Arrange
            var userDto = TestDataBuilder.CreateValidUserRegisterDto("existing@example.com");
            var baseUrl = "http://localhost";

            var existingUser = TestDataBuilder.CreateUserLogin("existing@example.com");
            await _dbContext.user_logins.AddAsync(existingUser);
            await _dbContext.SaveChangesAsync();

            _validatorMock.Setup(v => v.ValidateAsync(It.IsAny<UserRegisterDto>(), default))
                .ReturnsAsync(new ValidationResult());

            // Act
            var result = await _repository.CreatePendingUser(userDto, baseUrl);

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Error.Should().Be(ResponseMessages.EmailAlreadyRegistered);
        }

        [Fact]
        public async Task CreatePendingUser_WithExistingPendingUser_ShouldDeleteAndRecreate()
        {
            // Arrange
            var userDto = TestDataBuilder.CreateValidUserRegisterDto("pending@example.com");
            var baseUrl = "http://localhost";

            var existingPendingUser = TestDataBuilder.CreatePendingUser("pending@example.com");
            await _dbContext.pending_users.AddAsync(existingPendingUser);
            await _dbContext.SaveChangesAsync();

            _validatorMock.Setup(v => v.ValidateAsync(It.IsAny<UserRegisterDto>(), default))
                .ReturnsAsync(new ValidationResult());

            _passwordHasherMock.Setup(p => p.HashPassword(It.IsAny<string>()))
                .Returns("newhashedpassword");

            _otpServiceMock.Setup(o => o.GenerateAndSaveOtpAsync(It.IsAny<string>()))
                .ReturnsAsync(Result.Success("123456"));

            _emailServiceMock.Setup(e => e.SendOtpEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(true);

            // Act
            var result = await _repository.CreatePendingUser(userDto, baseUrl);

            // Assert
            result.IsSuccess.Should().BeTrue();

            var pendingUsers = await _dbContext.pending_users.Where(p => p.email == userDto.Email).ToListAsync();
            pendingUsers.Should().HaveCount(1);
        }

        [Fact]
        public async Task CreatePendingUser_WhenEmailFails_ShouldReturnFailure()
        {
            // Arrange
            var userDto = TestDataBuilder.CreateValidUserRegisterDto();
            var baseUrl = "http://localhost";

            _validatorMock.Setup(v => v.ValidateAsync(It.IsAny<UserRegisterDto>(), default))
                .ReturnsAsync(new ValidationResult());

            _passwordHasherMock.Setup(p => p.HashPassword(It.IsAny<string>()))
                .Returns("hashedpassword");

            _otpServiceMock.Setup(o => o.GenerateAndSaveOtpAsync(It.IsAny<string>()))
                .ReturnsAsync(Result.Success("123456"));

            _emailServiceMock.Setup(e => e.SendOtpEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(false);

            // Act
            var result = await _repository.CreatePendingUser(userDto, baseUrl);

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Error.Should().Be(OtpMessages.FailedToSendOtpEmail);
        }

        [Fact]
        public async Task ActivateUser_WithValidPendingUser_ShouldSucceed()
        {
            // Arrange
            var email = "test@example.com";
            var pendingUser = TestDataBuilder.CreatePendingUser(email);
            pendingUser.expires_at = DateTime.Now.AddMinutes(20);
            await _dbContext.pending_users.AddAsync(pendingUser);

            var otpCode = TestDataBuilder.CreateOtpCode(email);
            await _dbContext.otp_codes.AddAsync(otpCode);
            await _dbContext.SaveChangesAsync();

            // Act
            var result = await _repository.ActivateUser(email);

            // Assert
            result.IsSuccess.Should().BeTrue();

            var activatedUser = await _dbContext.user_logins.FirstOrDefaultAsync(u => u.email == email);
            activatedUser.Should().NotBeNull();
            activatedUser!.email.Should().Be(email);

            var pendingUserCheck = await _dbContext.pending_users.FirstOrDefaultAsync(p => p.email == email);
            pendingUserCheck.Should().BeNull();

            var otpCheck = await _dbContext.otp_codes.FirstOrDefaultAsync(o => o.email == email);
            otpCheck.Should().BeNull();
        }

        [Fact]
        public async Task ActivateUser_WithNonExistentPendingUser_ShouldFail()
        {
            // Arrange
            var email = "nonexistent@example.com";

            // Act
            var result = await _repository.ActivateUser(email);

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Error.Should().Be(OtpMessages.PendingUserDataNotFound);
        }

        [Fact]
        public async Task ActivateUser_WithExpiredRegistration_ShouldFail()
        {
            // Arrange
            var email = "expired@example.com";
            var pendingUser = TestDataBuilder.CreatePendingUser(email);
            pendingUser.expires_at = DateTime.Now.AddMinutes(-5); // Expired
            await _dbContext.pending_users.AddAsync(pendingUser);
            await _dbContext.SaveChangesAsync();

            // Act
            var result = await _repository.ActivateUser(email);

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Error.Should().Be(OtpMessages.RegistrationExpired);

            var pendingUserCheck = await _dbContext.pending_users.FirstOrDefaultAsync(p => p.email == email);
            pendingUserCheck.Should().BeNull();
        }

        public void Dispose()
        {
            _dbContext.Database.EnsureDeleted();
            _dbContext.Dispose();
        }
    }
}

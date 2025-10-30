using API.Configuration;
using API.Constants;
using API.Data;
using API.DTOs;
using API.Models;
using API.Services.Otp;
using API.Test.Helpers;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace API.Test.Services
{
    public class OtpServiceTests : IDisposable
    {
        private readonly IOtpService _otpService;
        private readonly Mock<ILogger<OtpService>> _loggerMock;
        private readonly ApplicationDbContext _dbContext;

        public OtpServiceTests()
        {
            _dbContext = TestDbContextFactory.CreateInMemoryContext(Guid.NewGuid().ToString());
            _loggerMock = new Mock<ILogger<OtpService>>();

            var otpOptions = Options.Create(new OtpOptions
            {
                CodeLength = 6,
                ExpirationMinutes = 10,
                MaxAttempts = 5,
                PendingUserExpirationMinutes = 30
            });

            _otpService = new OtpService(_dbContext, otpOptions, _loggerMock.Object);
        }

        [Fact]
        public async Task GenerateAndSaveOtpAsync_ShouldGenerateValidOtp()
        {
            // Arrange
            var email = "test@example.com";
            var pendingUser = TestDataBuilder.CreatePendingUser(email);
            await _dbContext.pending_users.AddAsync(pendingUser);
            await _dbContext.SaveChangesAsync();

            // Act
            var result = await _otpService.GenerateAndSaveOtpAsync(email);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().HaveLength(6);
            result.Value.Should().MatchRegex("^[0-9]{6}$");
        }

        [Fact]
        public async Task GenerateAndSaveOtpAsync_ShouldSaveOtpToDatabase()
        {
            // Arrange
            var email = "test@example.com";
            var pendingUser = TestDataBuilder.CreatePendingUser(email);
            await _dbContext.pending_users.AddAsync(pendingUser);
            await _dbContext.SaveChangesAsync();

            // Act
            var result = await _otpService.GenerateAndSaveOtpAsync(email);

            // Assert
            var savedOtp = await _dbContext.otp_codes.FirstOrDefaultAsync(o => o.email == email);
            savedOtp.Should().NotBeNull();
            savedOtp!.otp_code.Should().Be(result.Value);
            savedOtp.is_verified.Should().BeFalse();
            savedOtp.attempts.Should().Be(0);
        }

        [Fact]
        public async Task GenerateAndSaveOtpAsync_ShouldDeleteExistingOtp()
        {
            // Arrange
            var email = "test@example.com";
            var existingOtp = TestDataBuilder.CreateOtpCode(email, "111111");
            await _dbContext.otp_codes.AddAsync(existingOtp);
            await _dbContext.SaveChangesAsync();

            // Act
            var result = await _otpService.GenerateAndSaveOtpAsync(email);

            // Assert
            var otpCount = await _dbContext.otp_codes.CountAsync(o => o.email == email);
            otpCount.Should().Be(1);

            var savedOtp = await _dbContext.otp_codes.FirstOrDefaultAsync(o => o.email == email);
            savedOtp!.otp_code.Should().NotBe("111111");
        }

        [Fact]
        public async Task VerifyOtpAsync_WithValidOtp_ShouldSucceed()
        {
            // Arrange
            var email = "test@example.com";
            var otpCode = "123456";
            var otp = TestDataBuilder.CreateOtpCode(email, otpCode);
            otp.expires_at = DateTime.Now.AddMinutes(10);
            await _dbContext.otp_codes.AddAsync(otp);
            await _dbContext.SaveChangesAsync();

            var verifyDto = new VerifyOtpDto
            {
                Email = email,
                OtpCode = otpCode
            };

            // Act
            var result = await _otpService.VerifyOtpAsync(verifyDto);

            // Assert
            result.IsSuccess.Should().BeTrue();

            var verifiedOtp = await _dbContext.otp_codes.FirstOrDefaultAsync(o => o.email == email);
            verifiedOtp!.is_verified.Should().BeTrue();
        }

        [Fact]
        public async Task VerifyOtpAsync_WithInvalidOtp_ShouldFail()
        {
            // Arrange
            var email = "test@example.com";
            var otp = TestDataBuilder.CreateOtpCode(email, "123456");
            otp.expires_at = DateTime.Now.AddMinutes(10);
            await _dbContext.otp_codes.AddAsync(otp);
            await _dbContext.SaveChangesAsync();

            var verifyDto = new VerifyOtpDto
            {
                Email = email,
                OtpCode = "999999"
            };

            // Act
            var result = await _otpService.VerifyOtpAsync(verifyDto);

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Error.Should().Contain(OtpMessages.OtpInvalid);

            var otpRecord = await _dbContext.otp_codes.FirstOrDefaultAsync(o => o.email == email);
            otpRecord!.attempts.Should().Be(1);
        }

        [Fact]
        public async Task VerifyOtpAsync_WithExpiredOtp_ShouldFail()
        {
            // Arrange
            var email = "test@example.com";
            var otpCode = "123456";
            var otp = TestDataBuilder.CreateOtpCode(email, otpCode);
            otp.expires_at = DateTime.Now.AddMinutes(-5); // Expired
            await _dbContext.otp_codes.AddAsync(otp);
            await _dbContext.SaveChangesAsync();

            var verifyDto = new VerifyOtpDto
            {
                Email = email,
                OtpCode = otpCode
            };

            // Act
            var result = await _otpService.VerifyOtpAsync(verifyDto);

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Error.Should().Be(OtpMessages.OtpExpired);
        }

        [Fact]
        public async Task VerifyOtpAsync_WithMaxAttemptsReached_ShouldFail()
        {
            // Arrange
            var email = "test@example.com";
            var otpCode = "123456";
            var otp = TestDataBuilder.CreateOtpCode(email, otpCode);
            otp.expires_at = DateTime.Now.AddMinutes(10);
            otp.attempts = 5;
            await _dbContext.otp_codes.AddAsync(otp);
            await _dbContext.SaveChangesAsync();

            var verifyDto = new VerifyOtpDto
            {
                Email = email,
                OtpCode = otpCode
            };

            // Act
            var result = await _otpService.VerifyOtpAsync(verifyDto);

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Error.Should().Be(OtpMessages.OtpMaxAttemptsReached);
        }

        [Fact]
        public async Task VerifyOtpAsync_WithNonExistentOtp_ShouldFail()
        {
            // Arrange
            var verifyDto = new VerifyOtpDto
            {
                Email = "nonexistent@example.com",
                OtpCode = "123456"
            };

            // Act
            var result = await _otpService.VerifyOtpAsync(verifyDto);

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Error.Should().Be(OtpMessages.OtpNotFound);
        }

        [Fact]
        public async Task ResendOtpAsync_WithValidPendingUser_ShouldSucceed()
        {
            // Arrange
            var email = "test@example.com";
            var pendingUser = TestDataBuilder.CreatePendingUser(email);
            pendingUser.expires_at = DateTime.Now.AddMinutes(20);
            await _dbContext.pending_users.AddAsync(pendingUser);
            await _dbContext.SaveChangesAsync();

            // Act
            var result = await _otpService.ResendOtpAsync(email);

            // Assert
            result.IsSuccess.Should().BeTrue();

            var otpRecord = await _dbContext.otp_codes.FirstOrDefaultAsync(o => o.email == email);
            otpRecord.Should().NotBeNull();
        }

        [Fact]
        public async Task ResendOtpAsync_WithNonExistentPendingUser_ShouldFail()
        {
            // Arrange
            var email = "nonexistent@example.com";

            // Act
            var result = await _otpService.ResendOtpAsync(email);

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Error.Should().Be(OtpMessages.PendingUserNotFound);
        }

        [Fact]
        public async Task ResendOtpAsync_WithExpiredRegistration_ShouldFail()
        {
            // Arrange
            var email = "test@example.com";
            var pendingUser = TestDataBuilder.CreatePendingUser(email);
            pendingUser.expires_at = DateTime.Now.AddMinutes(-5); // Expired
            await _dbContext.pending_users.AddAsync(pendingUser);
            await _dbContext.SaveChangesAsync();

            // Act
            var result = await _otpService.ResendOtpAsync(email);

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Error.Should().Be(OtpMessages.RegistrationExpired);
        }

        public void Dispose()
        {
            _dbContext.Database.EnsureDeleted();
            _dbContext.Dispose();
        }
    }
}

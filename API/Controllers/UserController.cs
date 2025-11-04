using API.Constants;
using API.DTOs;
using API.Repositories.Register;
using API.Services.Otp;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace API.Controllers
{
    [Route("api/user")]
    [ApiController]
    public class UserController : BaseController
    {
        private readonly IRegisterRepository _registerRepository;
        private readonly IOtpService _otpService;

        public UserController(IRegisterRepository registerRepository, IOtpService otpService)
        {
            _registerRepository = registerRepository;
            _otpService = otpService;
        }

        [Route("register")]
        [HttpPost]
        public async Task<IActionResult> Register([FromForm] UserRegisterDto userRegister)
        {
            // Get base URL from request
            var baseUrl = $"{Request.Scheme}://{Request.Host}";

            var result = await _registerRepository.CreatePendingUser(userRegister, baseUrl);

            if (result.IsFailure)
            {
                return HandleResult(result);
            }

            return Ok(new
            {
                type = ResponseMessages.Success,
                message = result.Value.Message,
                data = new
                {
                    email = result.Value.Email,
                    expirationMinutes = result.Value.ExpirationMinutes
                }
            });
        }

        [Route("verify")]
        [HttpPost]
        public async Task<IActionResult> VerifyOtp([FromForm] VerifyOtpDto verifyOtpDto)
        {
            // Verify OTP
            var verifyResult = await _otpService.VerifyOtpAsync(verifyOtpDto);

            if (verifyResult.IsFailure)
            {
                return HandleResult(verifyResult);
            }

            // Activate user account
            var activateResult = await _registerRepository.ActivateUser(verifyOtpDto.Email);

            if (activateResult.IsFailure)
            {
                return HandleResult(activateResult);
            }

            return Ok(new
            {
                type = ResponseMessages.Success,
                message = OtpMessages.UserActivatedSuccessfully,
                data = new
                {
                    email = verifyOtpDto.Email
                }
            });
        }

        [Route("login")]
        [HttpPost]
        public async Task<IActionResult> Login([FromForm] LoginDto loginDto)
        {
            var result = await _registerRepository.Login(loginDto);

            if (result.IsFailure)
            {
                return HandleResult(result);
            }

            return Ok(new
            {
                type = ResponseMessages.Success,
                message = ResponseMessages.LoginSuccessful,
                data = result.Value
            });
        }

        [Route("auth/me")]
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetCurrentUser()
        {
            // Get user ID from JWT claims
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized(new
                {
                    type = ResponseMessages.Failed,
                    message = "Invalid token"
                });
            }

            var result = await _registerRepository.GetCurrentUser(userId);

            if (result.IsFailure)
            {
                return HandleResult(result);
            }

            return Ok(new
            {
                type = ResponseMessages.Success,
                message = "User information retrieved successfully",
                data = result.Value
            });
        }

        [Route("resend-otp")]
        [HttpPost]
        public async Task<IActionResult> ResendOtp([FromForm] ResendOtpDto resendOtpDto)
        {
            var result = await _otpService.ResendOtpAsync(resendOtpDto.Email);

            if (result.IsFailure)
            {
                return HandleResult(result);
            }

            return Ok(new
            {
                type = ResponseMessages.Success,
                message = OtpMessages.OtpResentSuccessfully,
                data = new
                {
                    email = resendOtpDto.Email
                }
            });
        }

        [Route("forgot-password")]
        [HttpPost]
        public async Task<IActionResult> ForgotPassword([FromForm] ForgotPasswordDto forgotPasswordDto)
        {
            var result = await _registerRepository.SendPasswordResetOtp(forgotPasswordDto.Email);

            if (result.IsFailure)
            {
                return HandleResult(result);
            }

            return Ok(new
            {
                type = ResponseMessages.Success,
                message = ResponseMessages.PasswordResetOtpSent,
                data = new
                {
                    email = forgotPasswordDto.Email
                }
            });
        }

        [Route("reset-password")]
        [HttpPost]
        public async Task<IActionResult> ResetPassword([FromForm] ResetPasswordDto resetPasswordDto)
        {
            var result = await _registerRepository.ResetPassword(resetPasswordDto);

            if (result.IsFailure)
            {
                return HandleResult(result);
            }

            return Ok(new
            {
                type = ResponseMessages.Success,
                message = ResponseMessages.PasswordResetSuccessful,
                data = new
                {
                    email = resetPasswordDto.Email
                }
            });
        }

        [Route("logout")]
        [HttpPost]
        [Authorize]
        public IActionResult Logout()
        {
            // In JWT authentication, logout is typically handled client-side
            // by removing the token. This endpoint confirms the logout action.
            return Ok(new
            {
                type = ResponseMessages.Success,
                message = "Logout successful"
            });
        }
    }
}

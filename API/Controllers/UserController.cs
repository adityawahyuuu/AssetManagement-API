using API.Constants;
using API.DTOs;
using API.Repositories.Register;
using API.Services.Otp;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Route("/user")]
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
    }
}

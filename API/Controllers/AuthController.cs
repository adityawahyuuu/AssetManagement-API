using API.Constants;
using API.DTOs;
using API.Repositories.Register;
using API.Services.Otp;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthController : BaseController
    {
        private readonly IRegisterRepository _registerRepository;
        private readonly IOtpService _otpService;

        public AuthController(IRegisterRepository registerRepository, IOtpService otpService)
        {
            _registerRepository = registerRepository;
            _otpService = otpService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] UserRegisterDto userRegister)
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
                    email = result.Value.Email
                }
            });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
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
    }
}

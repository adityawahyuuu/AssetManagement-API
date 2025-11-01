using API.Common;
using API.DTOs;

namespace API.Repositories.Register
{
    public interface IRegisterRepository
    {
        Task<Result<RegistrationResponseDto>> CreatePendingUser(UserRegisterDto userRegister, string baseUrl);
        Task<Result> ActivateUser(string email);
        Task<Result<LoginResponseDto>> Login(LoginDto loginDto);
        Task<Result<AuthMeResponseDto>> GetCurrentUser(int userId);
        Task<Result> SendPasswordResetOtp(string email);
        Task<Result> ResetPassword(ResetPasswordDto resetPasswordDto);
    }
}

using API.Common;
using API.DTOs;

namespace API.Services.Otp
{
    public interface IOtpService
    {
        Task<Result<string>> GenerateAndSaveOtpAsync(string email);
        Task<Result> VerifyOtpAsync(VerifyOtpDto verifyOtpDto);
        Task<Result> ResendOtpAsync(string email);
    }
}

using API.Common;
using API.DTOs;

namespace API.Repositories.Register
{
    public interface IRegisterRepository
    {
        Task<Result<RegistrationResponseDto>> CreatePendingUser(UserRegisterDto userRegister, string baseUrl);
        Task<Result> ActivateUser(string email);
    }
}

using API.Models;

namespace API.Services.Jwt
{
    public interface IJwtService
    {
        string GenerateToken(user_login user);
        int? ValidateTokenAndGetUserId(string token);
    }
}

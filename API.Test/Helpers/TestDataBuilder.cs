using API.DTOs;
using API.Models;
using System.Collections;

namespace API.Test.Helpers
{
    public static class TestDataBuilder
    {
        public static UserRegisterDto CreateValidUserRegisterDto(string email = "test@example.com")
        {
            return new UserRegisterDto
            {
                Email = email,
                Username = "testuser123",
                Password = "Password123!",
                PasswordConfirm = "Password123!"
            };
        }

        public static pending_users CreatePendingUser(string email = "test@example.com")
        {
            return new pending_users
            {
                email = email,
                username = "testuser123",
                password_hash = "hashedpassword",
                created_at = DateTime.Now,
                expires_at = DateTime.Now.AddMinutes(30)
            };
        }

        public static otp_codes CreateOtpCode(string email = "test@example.com", string otpCode = "123456")
        {
            return new otp_codes
            {
                email = email,
                otp_code = otpCode,
                created_at = DateTime.Now,
                expires_at = DateTime.Now.AddMinutes(10),
                is_verified = false,
                attempts = 0,
                max_attempts = 5
            };
        }

        public static user_login CreateUserLogin(string email = "test@example.com")
        {
            return new user_login
            {
                email = email,
                username = "testuser123",
                password_hash = "hashedpassword",
                created_at = DateTime.Now,
                updated_at = DateTime.Now,
                is_confirmed = new BitArray(1, true)
            };
        }
    }
}

namespace API.DTOs
{
    public class RegistrationResponseDto
    {
        public string Email { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public int ExpirationMinutes { get; set; }
    }
}

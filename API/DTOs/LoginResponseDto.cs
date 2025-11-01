namespace API.DTOs
{
    public class LoginResponseDto
    {
        public int UserId { get; set; }
        public string Email { get; set; } = string.Empty;
        public string? Username { get; set; }
        public DateTime? CreatedAt { get; set; }
        public string Token { get; set; } = string.Empty;
    }
}

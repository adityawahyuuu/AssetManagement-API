namespace API.DTOs
{
    public class AuthMeResponseDto
    {
        public int UserId { get; set; }
        public string Email { get; set; } = string.Empty;
        public string? Username { get; set; }
        public DateTime? CreatedAt { get; set; }
        public bool IsConfirmed { get; set; }
    }
}

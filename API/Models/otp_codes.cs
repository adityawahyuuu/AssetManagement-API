namespace API.Models;

public partial class otp_codes
{
    public int id { get; set; }

    public string email { get; set; } = null!;

    public string otp_code { get; set; } = null!;

    public DateTime? created_at { get; set; }

    public DateTime expires_at { get; set; }

    public bool is_verified { get; set; }

    public int attempts { get; set; }

    public int max_attempts { get; set; }
}

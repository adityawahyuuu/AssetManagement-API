namespace API.Models;

public partial class password_reset_tokens
{
    public int id { get; set; }

    public string email { get; set; } = null!;

    public string token { get; set; } = null!;

    public DateTime created_at { get; set; }

    public DateTime expires_at { get; set; }

    public bool is_used { get; set; }
}

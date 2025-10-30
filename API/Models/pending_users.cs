namespace API.Models;

public partial class pending_users
{
    public int id { get; set; }

    public string email { get; set; } = null!;

    public string username { get; set; } = null!;

    public string password_hash { get; set; } = null!;

    public DateTime? created_at { get; set; }

    public DateTime expires_at { get; set; }
}

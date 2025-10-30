﻿using System.Collections;

namespace API.Models;

public partial class user_login
{
    public int userid { get; set; }

    public string email { get; set; } = null!;

    public string password_hash { get; set; } = null!;

    public string? username { get; set; }

    public DateTime? created_at { get; set; }

    public DateTime? updated_at { get; set; }

    public Boolean? is_confirmed { get; set; }
}

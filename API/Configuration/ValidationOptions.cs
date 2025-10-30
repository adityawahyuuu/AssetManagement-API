namespace API.Configuration
{
    public class ValidationOptions
    {
        public const string SectionName = "Validation";

        public UsernameValidation Username { get; set; } = new();
        public PasswordValidation Password { get; set; } = new();
    }

    public class UsernameValidation
    {
        public int MinLength { get; set; } = 10;
        public int MaxLength { get; set; } = 50;
    }

    public class PasswordValidation
    {
        public int MinLength { get; set; } = 8;
        public int MaxLength { get; set; } = 16;
        public bool RequireUppercase { get; set; } = true;
        public bool RequireLowercase { get; set; } = true;
        public bool RequireDigit { get; set; } = true;
        public bool RequireSpecialChar { get; set; } = true;
        public string SpecialCharPattern { get; set; } = @"[\!\?\*\.]+";
    }
}

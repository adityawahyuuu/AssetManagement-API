namespace API.Configuration
{
    public class PasswordResetOptions
    {
        public const string SectionName = "PasswordReset";

        public int TokenExpirationMinutes { get; set; } = 15;
        public int CodeLength { get; set; } = 6;
    }
}

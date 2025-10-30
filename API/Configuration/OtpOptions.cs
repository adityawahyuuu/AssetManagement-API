namespace API.Configuration
{
    public class OtpOptions
    {
        public const string SectionName = "Otp";

        public int CodeLength { get; set; } = 6;
        public int ExpirationMinutes { get; set; } = 10;
        public int MaxAttempts { get; set; } = 5;
        public int PendingUserExpirationMinutes { get; set; } = 30;
    }
}

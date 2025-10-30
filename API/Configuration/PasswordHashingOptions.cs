namespace API.Configuration
{
    public class PasswordHashingOptions
    {
        public const string SectionName = "PasswordHashing";

        public int SaltSize { get; set; } = 16;
        public int HashSize { get; set; } = 32;
        public int Iterations { get; set; } = 10000;
        public string HashAlgorithm { get; set; } = "SHA256";
    }
}

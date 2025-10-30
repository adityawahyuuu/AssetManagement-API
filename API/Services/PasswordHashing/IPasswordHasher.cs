namespace API.Services.PasswordHashing
{
    public interface IPasswordHasher
    {
        public string HashPassword(string password);
    }
}

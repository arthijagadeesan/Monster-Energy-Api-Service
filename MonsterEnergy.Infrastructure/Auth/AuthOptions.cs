namespace MonsterEnergy.Infrastructure.Auth
{
    public class AuthOptions
    {
        public const string SectionName = "Auth";

        public List<AuthUser> Users { get; set; } = [];
    }

    public class AuthUser
    {
        public string Username { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
    }
}

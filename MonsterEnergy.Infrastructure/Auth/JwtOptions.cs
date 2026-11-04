using Microsoft.IdentityModel.Tokens;

namespace MonsterEnergy.Infrastructure.Auth
{
    public class JwtOptions
    {
        public const string SectionName = "Jwt";

        public string Issuer { get; set; } = string.Empty;
        public string Audience { get; set; } = string.Empty;
        public string SigningKey { get; set; } = string.Empty;
        public int ExpiryMinutes { get; set; } = 60;

        public SymmetricSecurityKey GetSigningKey()
        {
            return new SymmetricSecurityKey(Convert.FromBase64String(SigningKey));
        }
    }
}

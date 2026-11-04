using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using MonsterEnergy.Application.Dtos;
using MonsterEnergy.Application.Interfaces;

namespace MonsterEnergy.Infrastructure.Auth
{
    public class AuthService(IOptions<JwtOptions> jwtOptions, IOptions<AuthOptions> authOptions) : IAuthService
    {
        private readonly PasswordHasher<AuthUser> _passwordHasher = new();
        private readonly JsonWebTokenHandler _tokenHandler = new();

        public Task<LoginResponse?> LoginAsync(LoginRequest request, CancellationToken ct)
        {
            var user = authOptions.Value.Users
                .FirstOrDefault(u => string.Equals(u.Username, request.Username, StringComparison.OrdinalIgnoreCase));

            if (user is null)
            {
                return Task.FromResult<LoginResponse?>(null);
            }

            var result = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);
            if (result == PasswordVerificationResult.Failed)
            {
                return Task.FromResult<LoginResponse?>(null);
            }

            return Task.FromResult<LoginResponse?>(CreateToken(user.Username));
        }

        private LoginResponse CreateToken(string username)
        {
            var jwt = jwtOptions.Value;
            var now = DateTime.UtcNow;
            var expiresAt = now.AddMinutes(jwt.ExpiryMinutes);

            var descriptor = new SecurityTokenDescriptor
            {
                Issuer = jwt.Issuer,
                Audience = jwt.Audience,
                Subject = new ClaimsIdentity(
                [
                    new Claim(JwtRegisteredClaimNames.Sub, username),
                    new Claim(JwtRegisteredClaimNames.Name, username),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                ]),
                IssuedAt = now,
                NotBefore = now,
                Expires = expiresAt,
                SigningCredentials = new SigningCredentials(jwt.GetSigningKey(), SecurityAlgorithms.HmacSha256),
            };

            var token = _tokenHandler.CreateToken(descriptor);

            return new LoginResponse(token, "Bearer", jwt.ExpiryMinutes * 60, expiresAt);
        }
    }
}

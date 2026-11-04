using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using MonsterEnergy.Application.Dtos;
using MonsterEnergy.Infrastructure.Auth;

namespace MonsterEnergy.Tests.Api
{
    public class AuthApiTests(ApiFactory factory) : IClassFixture<ApiFactory>
    {
        [Fact]
        public async Task Login_ValidCredentials_ReturnsTokenValidForOneHour()
        {
            var client = factory.CreateClient();

            var response = await client.PostAsJsonAsync("/api/auth/login", new { ApiFactory.Username, ApiFactory.Password });

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var login = await response.Content.ReadFromJsonAsync<LoginResponse>();
            Assert.NotNull(login);
            Assert.False(string.IsNullOrEmpty(login.AccessToken));
            Assert.Equal("Bearer", login.TokenType);
            Assert.Equal(3600, login.ExpiresIn);
            Assert.InRange(login.ExpiresAt, DateTime.UtcNow.AddMinutes(59), DateTime.UtcNow.AddMinutes(61));

            var token = new JsonWebToken(login.AccessToken);
            Assert.Equal(ApiFactory.Username, token.Subject);
            Assert.Equal(login.ExpiresAt, token.ValidTo, TimeSpan.FromSeconds(1));
        }

        [Fact]
        public async Task Login_WrongPassword_ReturnsUnauthorizedProblem()
        {
            var client = factory.CreateClient();

            var response = await client.PostAsJsonAsync("/api/auth/login", new { ApiFactory.Username, Password = "wrong-password" });

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
            Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);

            using var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
            Assert.Equal("Invalid credentials", body.RootElement.GetProperty("title").GetString());
        }

        [Fact]
        public async Task Login_MissingFields_ReturnsValidationProblem()
        {
            var client = factory.CreateClient();

            var response = await client.PostAsJsonAsync("/api/auth/login", new { Username = "", Password = "" });

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

            using var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
            var errors = body.RootElement.GetProperty("errors");
            Assert.True(errors.TryGetProperty("Username", out _));
            Assert.True(errors.TryGetProperty("Password", out _));
        }

        [Fact]
        public async Task Stores_WithoutToken_ReturnsUnauthorized()
        {
            var client = factory.CreateClient();

            var response = await client.GetAsync("/api/stores");

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task Stores_WithInvalidToken_ReturnsUnauthorized()
        {
            var client = factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "not-a-real-token");

            var response = await client.GetAsync("/api/stores");

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task Stores_WithExpiredToken_ReturnsUnauthorized()
        {
            var client = factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", CreateExpiredToken());

            var response = await client.GetAsync("/api/stores");

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task Stores_WithValidToken_ReturnsOk()
        {
            var client = await factory.CreateAuthenticatedClientAsync();

            var response = await client.GetAsync("/api/stores");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        private string CreateExpiredToken()
        {
            var jwt = factory.Services.GetRequiredService<IOptions<JwtOptions>>().Value;
            var issuedAt = DateTime.UtcNow.AddHours(-2);

            return new JsonWebTokenHandler().CreateToken(new SecurityTokenDescriptor
            {
                Issuer = jwt.Issuer,
                Audience = jwt.Audience,
                Subject = new ClaimsIdentity([new Claim(JwtRegisteredClaimNames.Sub, ApiFactory.Username)]),
                IssuedAt = issuedAt,
                NotBefore = issuedAt,
                Expires = issuedAt.AddHours(1),
                SigningCredentials = new SigningCredentials(jwt.GetSigningKey(), SecurityAlgorithms.HmacSha256),
            });
        }
    }
}

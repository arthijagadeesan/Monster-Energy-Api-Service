using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using MonsterEnergy.Application.Dtos;

namespace MonsterEnergy.Tests.Api
{
    public class ApiFactory : WebApplicationFactory<Program>
    {
        public const string Username = "admin";
        public const string Password = "Monster@123";

        private readonly string _databasePath = Path.Combine(Path.GetTempPath(), $"monsterenergy-tests-{Guid.NewGuid()}.db");

        public async Task<HttpClient> CreateAuthenticatedClientAsync()
        {
            var client = CreateClient();
            var response = await client.PostAsJsonAsync("/api/auth/login", new { Username, Password });
            response.EnsureSuccessStatusCode();

            var login = await response.Content.ReadFromJsonAsync<LoginResponse>();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", login!.AccessToken);
            return client;
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseSetting("ConnectionStrings:Default", $"Data Source={_databasePath}");
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            SqliteConnection.ClearAllPools();

            if (File.Exists(_databasePath))
            {
                File.Delete(_databasePath);
            }
        }
    }
}

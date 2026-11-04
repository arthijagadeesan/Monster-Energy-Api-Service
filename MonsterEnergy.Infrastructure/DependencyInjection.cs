using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MonsterEnergy.Application.Interfaces;
using MonsterEnergy.Infrastructure.Auth;
using MonsterEnergy.Infrastructure.Database;
using MonsterEnergy.Infrastructure.Database.Repositories;

namespace MonsterEnergy.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration config)
        {
            services.AddDbContext<AppDbContext>(o => o.UseSqlite(config.GetConnectionString("Default")));
            services.AddScoped<IStoreRepository, StoreRepository>();

            services.Configure<JwtOptions>(config.GetSection(JwtOptions.SectionName));
            services.Configure<AuthOptions>(config.GetSection(AuthOptions.SectionName));
            services.AddScoped<IAuthService, AuthService>();

            return services;
        }
    }
}

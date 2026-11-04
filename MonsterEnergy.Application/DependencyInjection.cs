using Microsoft.Extensions.DependencyInjection;
using MonsterEnergy.Application.Interfaces;
using MonsterEnergy.Application.Services;

namespace MonsterEnergy.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            services.AddScoped<IStoreService, StoreService>();
            return services;
        }
    }
}

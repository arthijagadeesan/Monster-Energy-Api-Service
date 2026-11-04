using MonsterEnergy.Application.Dtos;

namespace MonsterEnergy.Application.Interfaces
{
    public interface IAuthService
    {
        Task<LoginResponse?> LoginAsync(LoginRequest request, CancellationToken ct);
    }
}

using MonsterEnergy.Application.Dtos;

namespace MonsterEnergy.Application.Interfaces
{
    public interface IStoreService
    {
        Task<PagedResult<StoreDto>> SearchAsync(StoreQuery query, string username, CancellationToken ct);
        Task<StoreDto?> GetByIdAsync(Guid storeGuid, string username, CancellationToken ct);
        Task<IReadOnlyList<string>> GetRegionsAsync(CancellationToken ct);
        Task<IReadOnlyList<string>> GetChannelsAsync(CancellationToken ct);
        Task<bool> SetFavouriteAsync(Guid storeGuid, string username, bool isFavourite, CancellationToken ct);
    }
}

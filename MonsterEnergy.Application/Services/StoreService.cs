using System.Globalization;
using MonsterEnergy.Application.Dtos;
using MonsterEnergy.Application.Interfaces;

namespace MonsterEnergy.Application.Services
{
    public class StoreService(IStoreRepository storeRepository) : IStoreService
    {
        public async Task<PagedResult<StoreDto>> SearchAsync(StoreQuery query, string username, CancellationToken ct)
        {
            var result = await storeRepository.SearchAsync(query, username, ct);
            var items = result.Items.Select(ToDto).ToList();

            return new PagedResult<StoreDto>(items, result.TotalCount, result.Page, result.PageSize);
        }

        public async Task<StoreDto?> GetByIdAsync(Guid storeGuid, string username, CancellationToken ct)
        {
            var store = await storeRepository.GetByIdAsync(storeGuid, username, ct);
            return store is null ? null : ToDto(store);
        }

        public async Task<IReadOnlyList<string>> GetRegionsAsync(CancellationToken ct)
        {
            return await storeRepository.GetRegionsAsync(ct);
        }

        public async Task<IReadOnlyList<string>> GetChannelsAsync(CancellationToken ct)
        {
            return await storeRepository.GetChannelsAsync(ct);
        }

        public async Task<bool> SetFavouriteAsync(Guid storeGuid, string username, bool isFavourite, CancellationToken ct)
        {
            return await storeRepository.SetFavouriteAsync(storeGuid, username, isFavourite, ct);
        }

        private static StoreDto ToDto(StoreWithFavourite item)
        {
            var s = item.Store;

            return new StoreDto(
                s.StoreGuid,
                s.Tdm,
                s.Territory,
                s.Region,
                s.Date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                s.StoreName,
                s.Address,
                s.PostCode,
                s.StoreStart.ToString("HH:mm", CultureInfo.InvariantCulture),
                s.StoreChannel,
                s.StoreFascia,
                item.IsFavourite);
        }
    }
}

using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using MonsterEnergy.Application.Dtos;
using MonsterEnergy.Application.Interfaces;
using MonsterEnergy.Domain.Entities;

namespace MonsterEnergy.Infrastructure.Database.Repositories
{
    public class StoreRepository(AppDbContext db) : IStoreRepository
    {
        public async Task<PagedResult<StoreWithFavourite>> SearchAsync(StoreQuery query, string username, CancellationToken ct)
        {
            IQueryable<Store> stores = db.Stores.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(query.Search))
            {
                var pattern = $"%{EscapeLike(query.Search.Trim())}%";
                stores = stores.Where(s =>
                    EF.Functions.Like(s.StoreName, pattern, "\\") ||
                    EF.Functions.Like(s.Address, pattern, "\\") ||
                    EF.Functions.Like(s.PostCode, pattern, "\\") ||
                    EF.Functions.Like(s.Tdm, pattern, "\\") ||
                    EF.Functions.Like(s.Territory, pattern, "\\") ||
                    EF.Functions.Like(s.StoreFascia, pattern, "\\"));
            }

            if (!string.IsNullOrWhiteSpace(query.Region))
            {
                var region = query.Region.Trim();
                stores = stores.Where(s => s.Region == region);
            }

            if (!string.IsNullOrWhiteSpace(query.Channel))
            {
                var channel = query.Channel.Trim();
                stores = stores.Where(s => s.StoreChannel == channel);
            }

            if (query.FavouritesOnly)
            {
                stores = stores.Where(s => db.UserFavourites.Any(f => f.Username == username && f.StoreGuid == s.StoreGuid));
            }

            var totalCount = await stores.CountAsync(ct);

            var descending = query.SortDir == "desc";
            var sorted = query.SortBy switch
            {
                "storeName" => Sort(stores, s => s.StoreName, descending),
                "tdm" => Sort(stores, s => s.Tdm, descending),
                "region" => Sort(stores, s => s.Region, descending),
                "storeChannel" => Sort(stores, s => s.StoreChannel, descending),
                "storeStart" => Sort(stores, s => s.StoreStart, descending),
                _ => Sort(stores, s => s.Date, descending),
            };

            var items = await sorted
                .ThenBy(s => s.StoreGuid)
                .Skip((query.Page - 1) * query.PageSize)
                .Take(query.PageSize)
                .Select(s => new StoreWithFavourite(
                    s,
                    db.UserFavourites.Any(f => f.Username == username && f.StoreGuid == s.StoreGuid)))
                .ToListAsync(ct);

            return new PagedResult<StoreWithFavourite>(items, totalCount, query.Page, query.PageSize);
        }

        public async Task<StoreWithFavourite?> GetByIdAsync(Guid storeGuid, string username, CancellationToken ct)
        {
            return await db.Stores
                .AsNoTracking()
                .Where(s => s.StoreGuid == storeGuid)
                .Select(s => new StoreWithFavourite(
                    s,
                    db.UserFavourites.Any(f => f.Username == username && f.StoreGuid == s.StoreGuid)))
                .FirstOrDefaultAsync(ct);
        }

        public async Task<IReadOnlyList<string>> GetRegionsAsync(CancellationToken ct)
        {
            return await db.Stores
                .AsNoTracking()
                .Select(s => s.Region)
                .Distinct()
                .OrderBy(r => r)
                .ToListAsync(ct);
        }

        public async Task<IReadOnlyList<string>> GetChannelsAsync(CancellationToken ct)
        {
            return await db.Stores
                .AsNoTracking()
                .Select(s => s.StoreChannel)
                .Distinct()
                .OrderBy(c => c)
                .ToListAsync(ct);
        }

        public async Task<bool> SetFavouriteAsync(Guid storeGuid, string username, bool isFavourite, CancellationToken ct)
        {
            var storeExists = await db.Stores.AnyAsync(s => s.StoreGuid == storeGuid, ct);
            if (!storeExists)
            {
                return false;
            }

            var favourite = await db.UserFavourites
                .FirstOrDefaultAsync(f => f.Username == username && f.StoreGuid == storeGuid, ct);

            if (isFavourite && favourite is null)
            {
                db.UserFavourites.Add(new UserFavourite
                {
                    Username = username,
                    StoreGuid = storeGuid,
                    CreatedAt = DateTime.UtcNow,
                });
            }
            else if (!isFavourite && favourite is not null)
            {
                db.UserFavourites.Remove(favourite);
            }

            await db.SaveChangesAsync(ct);
            return true;
        }

        private static IOrderedQueryable<Store> Sort<TKey>(IQueryable<Store> stores, Expression<Func<Store, TKey>> key, bool descending)
        {
            return descending ? stores.OrderByDescending(key) : stores.OrderBy(key);
        }

        private static string EscapeLike(string value)
        {
            return value.Replace("\\", "\\\\").Replace("%", "\\%").Replace("_", "\\_");
        }
    }
}

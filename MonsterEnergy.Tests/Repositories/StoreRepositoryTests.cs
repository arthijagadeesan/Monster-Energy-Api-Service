using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using MonsterEnergy.Application.Dtos;
using MonsterEnergy.Domain.Entities;
using MonsterEnergy.Infrastructure.Database;
using MonsterEnergy.Infrastructure.Database.Repositories;

namespace MonsterEnergy.Tests.Repositories
{
    public class StoreRepositoryTests : IAsyncLifetime
    {
        private const string Admin = "admin";
        private const string OtherUser = "other";

        private readonly SqliteConnection _connection = new("Data Source=:memory:");
        private AppDbContext _db = null!;
        private StoreRepository _repository = null!;

        public async Task InitializeAsync()
        {
            await _connection.OpenAsync();

            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseSqlite(_connection)
                .Options;

            _db = new AppDbContext(options);
            await _db.Database.EnsureCreatedAsync();

            var zabka = CreateStore("Żabka Gdynia", "Gdansk Region", "Convenience", "Katarzyna Lis", new DateOnly(2026, 4, 9), new TimeOnly(11, 0));

            _db.Stores.AddRange(
                CreateStore("Kiosk Ruch Sopot", "Gdansk Region", "Traditional Trade / Kiosk", "Paweł Górski", new DateOnly(2026, 4, 6), new TimeOnly(6, 1)),
                CreateStore("Stokrotka Legionowo", "Warszawa Region", "Supermarket", "Marek Zieliński", new DateOnly(2026, 4, 7), new TimeOnly(9, 30)),
                CreateStore("Polomarket Poznań", "Poznan Region", "Supermarket", "Michał Dąbrowski", new DateOnly(2026, 4, 8), new TimeOnly(7, 15)),
                zabka,
                CreateStore("Lewiatan Kraków", "Krakow Region", "Convenience", "Ewa Szymańska", new DateOnly(2026, 4, 10), new TimeOnly(8, 45)));

            _db.UserFavourites.Add(new UserFavourite { Username = Admin, StoreGuid = zabka.StoreGuid, CreatedAt = DateTime.UtcNow });

            await _db.SaveChangesAsync();
            _db.ChangeTracker.Clear();

            _repository = new StoreRepository(_db);
        }

        public async Task DisposeAsync()
        {
            await _db.DisposeAsync();
            await _connection.DisposeAsync();
        }

        [Fact]
        public async Task SearchAsync_FiltersByRegion()
        {
            var result = await _repository.SearchAsync(new StoreQuery { Region = "Gdansk Region" }, Admin, CancellationToken.None);

            Assert.Equal(2, result.TotalCount);
            Assert.All(result.Items, s => Assert.Equal("Gdansk Region", s.Store.Region));
        }

        [Fact]
        public async Task SearchAsync_FiltersByChannel()
        {
            var result = await _repository.SearchAsync(new StoreQuery { Channel = "Supermarket" }, Admin, CancellationToken.None);

            Assert.Equal(2, result.TotalCount);
            Assert.All(result.Items, s => Assert.Equal("Supermarket", s.Store.StoreChannel));
        }

        [Fact]
        public async Task SearchAsync_SearchIsCaseInsensitive()
        {
            var result = await _repository.SearchAsync(new StoreQuery { Search = "KIOSK" }, Admin, CancellationToken.None);

            var item = Assert.Single(result.Items);
            Assert.Equal("Kiosk Ruch Sopot", item.Store.StoreName);
        }

        [Fact]
        public async Task SearchAsync_TreatsWildcardCharactersAsText()
        {
            var result = await _repository.SearchAsync(new StoreQuery { Search = "%" }, Admin, CancellationToken.None);

            Assert.Equal(0, result.TotalCount);
        }

        [Fact]
        public async Task SearchAsync_SortsByStartTimeDescending()
        {
            var result = await _repository.SearchAsync(
                new StoreQuery { SortBy = "storeStart", SortDir = "desc" },
                Admin,
                CancellationToken.None);

            var startTimes = result.Items.Select(s => s.Store.StoreStart).ToList();
            Assert.Equal(startTimes.OrderByDescending(t => t), startTimes);
            Assert.Equal(new TimeOnly(11, 0), startTimes[0]);
        }

        [Fact]
        public async Task SearchAsync_ReturnsRequestedPageAndTotalCount()
        {
            var result = await _repository.SearchAsync(
                new StoreQuery { SortBy = "date", Page = 2, PageSize = 2 },
                Admin,
                CancellationToken.None);

            Assert.Equal(5, result.TotalCount);
            Assert.Equal(3, result.TotalPages);
            Assert.Equal(2, result.Items.Count);
            Assert.Equal(new DateOnly(2026, 4, 8), result.Items[0].Store.Date);
            Assert.Equal(new DateOnly(2026, 4, 9), result.Items[1].Store.Date);
        }

        [Fact]
        public async Task SearchAsync_PageBeyondLastPage_ReturnsEmptyItemsWithTotalCount()
        {
            var result = await _repository.SearchAsync(new StoreQuery { Page = 10, PageSize = 10 }, Admin, CancellationToken.None);

            Assert.Empty(result.Items);
            Assert.Equal(5, result.TotalCount);
        }

        [Fact]
        public async Task SearchAsync_FavouritesOnly_ReturnsOnlyThatUsersFavourites()
        {
            var result = await _repository.SearchAsync(new StoreQuery { FavouritesOnly = true }, Admin, CancellationToken.None);

            var item = Assert.Single(result.Items);
            Assert.Equal("Żabka Gdynia", item.Store.StoreName);
            Assert.True(item.IsFavourite);
        }

        [Fact]
        public async Task SearchAsync_FavouritesArePerUser()
        {
            var result = await _repository.SearchAsync(new StoreQuery { FavouritesOnly = true }, OtherUser, CancellationToken.None);

            Assert.Empty(result.Items);
        }

        [Fact]
        public async Task GetByIdAsync_ReturnsStoreWithFavouriteFlag()
        {
            var zabka = await _db.Stores.AsNoTracking().FirstAsync(s => s.StoreName == "Żabka Gdynia");

            var forAdmin = await _repository.GetByIdAsync(zabka.StoreGuid, Admin, CancellationToken.None);
            var forOther = await _repository.GetByIdAsync(zabka.StoreGuid, OtherUser, CancellationToken.None);

            Assert.NotNull(forAdmin);
            Assert.True(forAdmin.IsFavourite);
            Assert.NotNull(forOther);
            Assert.False(forOther.IsFavourite);
        }

        [Fact]
        public async Task GetByIdAsync_ReturnsNull_WhenStoreDoesNotExist()
        {
            var result = await _repository.GetByIdAsync(Guid.NewGuid(), Admin, CancellationToken.None);

            Assert.Null(result);
        }

        [Fact]
        public async Task GetRegionsAsync_ReturnsDistinctSortedRegions()
        {
            var regions = await _repository.GetRegionsAsync(CancellationToken.None);

            Assert.Equal(["Gdansk Region", "Krakow Region", "Poznan Region", "Warszawa Region"], regions);
        }

        [Fact]
        public async Task GetChannelsAsync_ReturnsDistinctSortedChannels()
        {
            var channels = await _repository.GetChannelsAsync(CancellationToken.None);

            Assert.Equal(["Convenience", "Supermarket", "Traditional Trade / Kiosk"], channels);
        }

        [Fact]
        public async Task SetFavouriteAsync_AddsAndRemovesFavourite()
        {
            var store = await _db.Stores.AsNoTracking().FirstAsync(s => s.StoreName == "Kiosk Ruch Sopot");

            Assert.True(await _repository.SetFavouriteAsync(store.StoreGuid, OtherUser, true, CancellationToken.None));
            Assert.True(await _db.UserFavourites.AnyAsync(f => f.Username == OtherUser && f.StoreGuid == store.StoreGuid));

            Assert.True(await _repository.SetFavouriteAsync(store.StoreGuid, OtherUser, false, CancellationToken.None));
            Assert.False(await _db.UserFavourites.AnyAsync(f => f.Username == OtherUser && f.StoreGuid == store.StoreGuid));
        }

        [Fact]
        public async Task SetFavouriteAsync_IsIdempotent()
        {
            var store = await _db.Stores.AsNoTracking().FirstAsync(s => s.StoreName == "Kiosk Ruch Sopot");

            await _repository.SetFavouriteAsync(store.StoreGuid, Admin, true, CancellationToken.None);
            _db.ChangeTracker.Clear();
            await _repository.SetFavouriteAsync(store.StoreGuid, Admin, true, CancellationToken.None);

            Assert.Equal(1, await _db.UserFavourites.CountAsync(f => f.Username == Admin && f.StoreGuid == store.StoreGuid));
        }

        [Fact]
        public async Task SetFavouriteAsync_ReturnsFalse_WhenStoreDoesNotExist()
        {
            var updated = await _repository.SetFavouriteAsync(Guid.NewGuid(), Admin, true, CancellationToken.None);

            Assert.False(updated);
        }

        private static Store CreateStore(string name, string region, string channel, string tdm, DateOnly date, TimeOnly start)
        {
            return new Store
            {
                StoreGuid = Guid.NewGuid(),
                StoreName = name,
                Region = region,
                StoreChannel = channel,
                Tdm = tdm,
                Territory = $"{tdm}'s Territory",
                Address = "Lipowa 1",
                PostCode = "00-001",
                StoreFascia = "Independent",
                Date = date,
                StoreStart = start,
            };
        }
    }
}

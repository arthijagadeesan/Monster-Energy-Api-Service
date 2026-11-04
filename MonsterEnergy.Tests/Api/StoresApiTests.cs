using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using MonsterEnergy.Application.Dtos;

namespace MonsterEnergy.Tests.Api
{
    public class StoresApiTests(ApiFactory factory) : IClassFixture<ApiFactory>, IAsyncLifetime
    {
        private HttpClient _client = null!;

        public async Task InitializeAsync()
        {
            _client = await factory.CreateAuthenticatedClientAsync();
        }

        public Task DisposeAsync()
        {
            _client.Dispose();
            return Task.CompletedTask;
        }

        [Fact]
        public async Task GetStores_ReturnsFirstPageOfSeededStores()
        {
            var result = await _client.GetFromJsonAsync<PagedResult<StoreDto>>("/api/stores?pageSize=10");

            Assert.NotNull(result);
            Assert.Equal(300, result.TotalCount);
            Assert.Equal(10, result.Items.Count);
            Assert.All(result.Items, s => Assert.False(string.IsNullOrEmpty(s.StoreName)));
        }

        [Fact]
        public async Task GetStores_ReturnsDateAndTimeInExpectedFormat()
        {
            var result = await _client.GetFromJsonAsync<PagedResult<StoreDto>>("/api/stores?sortBy=date&sortDir=asc&pageSize=1");

            var store = Assert.Single(result!.Items);
            Assert.Matches(@"^\d{4}-\d{2}-\d{2}$", store.Date);
            Assert.Matches(@"^\d{2}:\d{2}$", store.StoreStart);
        }

        [Theory]
        [InlineData("pageSize=0", "PageSize")]
        [InlineData("pageSize=500", "PageSize")]
        [InlineData("page=0", "Page")]
        [InlineData("sortBy=password", "SortBy")]
        [InlineData("sortDir=sideways", "SortDir")]
        public async Task GetStores_InvalidQuery_ReturnsValidationProblem(string queryString, string field)
        {
            var response = await _client.GetAsync($"/api/stores?{queryString}");

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);

            using var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
            Assert.True(body.RootElement.GetProperty("errors").TryGetProperty(field, out _));
        }

        [Fact]
        public async Task GetRegions_ReturnsAllRegions()
        {
            var regions = await _client.GetFromJsonAsync<List<string>>("/api/stores/regions");

            Assert.NotNull(regions);
            Assert.Equal(5, regions.Count);
            Assert.Contains("Gdansk Region", regions);
        }

        [Fact]
        public async Task GetChannels_ReturnsAllChannels()
        {
            var channels = await _client.GetFromJsonAsync<List<string>>("/api/stores/channels");

            Assert.NotNull(channels);
            Assert.Equal(5, channels.Count);
            Assert.Contains("Supermarket", channels);
        }

        [Fact]
        public async Task GetStores_FiltersByChannel()
        {
            var result = await _client.GetFromJsonAsync<PagedResult<StoreDto>>("/api/stores?channel=Hypermarket&pageSize=100");

            Assert.NotNull(result);
            Assert.NotEmpty(result.Items);
            Assert.All(result.Items, s => Assert.Equal("Hypermarket", s.StoreChannel));
        }

        [Fact]
        public async Task GetStore_ReturnsStoreDetail()
        {
            var page = await _client.GetFromJsonAsync<PagedResult<StoreDto>>("/api/stores?pageSize=1");
            var expected = Assert.Single(page!.Items);

            var store = await _client.GetFromJsonAsync<StoreDto>($"/api/stores/{expected.StoreGuid}");

            Assert.Equal(expected, store);
        }

        [Fact]
        public async Task GetStore_UnknownStore_ReturnsNotFoundProblem()
        {
            var response = await _client.GetAsync($"/api/stores/{Guid.NewGuid()}");

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
        }

        [Fact]
        public async Task AddFavourite_UnknownStore_ReturnsNotFoundProblem()
        {
            var response = await _client.PutAsync($"/api/stores/{Guid.NewGuid()}/favourite", null);

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);

            using var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
            Assert.Equal("Store not found", body.RootElement.GetProperty("title").GetString());
        }

        [Fact]
        public async Task AddAndRemoveFavourite_UpdatesFavouritesFilter()
        {
            var page = await _client.GetFromJsonAsync<PagedResult<StoreDto>>("/api/stores?search=Sopot&pageSize=1");
            var store = Assert.Single(page!.Items);

            var added = await _client.PutAsync($"/api/stores/{store.StoreGuid}/favourite", null);
            Assert.Equal(HttpStatusCode.NoContent, added.StatusCode);

            var favourites = await _client.GetFromJsonAsync<PagedResult<StoreDto>>("/api/stores?favouritesOnly=true&pageSize=100");
            Assert.Contains(favourites!.Items, s => s.StoreGuid == store.StoreGuid);

            var removed = await _client.DeleteAsync($"/api/stores/{store.StoreGuid}/favourite");
            Assert.Equal(HttpStatusCode.NoContent, removed.StatusCode);

            favourites = await _client.GetFromJsonAsync<PagedResult<StoreDto>>("/api/stores?favouritesOnly=true&pageSize=100");
            Assert.DoesNotContain(favourites!.Items, s => s.StoreGuid == store.StoreGuid);
        }
    }
}

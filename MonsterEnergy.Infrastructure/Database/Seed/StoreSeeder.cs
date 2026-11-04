using System.Globalization;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using MonsterEnergy.Domain.Entities;

namespace MonsterEnergy.Infrastructure.Database.Seed
{
    public static class StoreSeeder
    {
        public static async Task SeedAsync(AppDbContext db, string jsonPath, CancellationToken ct = default)
        {
            await db.Database.MigrateAsync(ct);

            if (await db.Stores.AnyAsync(ct))
            {
                return;
            }

            await using var stream = File.OpenRead(jsonPath);
            var records = await JsonSerializer.DeserializeAsync<List<StoreSeedRecord>>(
                stream,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true },
                ct);

            if (records is null || records.Count == 0)
            {
                return;
            }

            var stores = records.Select(r => new Store
            {
                StoreGuid = Guid.Parse(r.StoreGuid),
                Tdm = r.Tdm,
                Territory = r.Territory,
                Region = r.Region,
                Date = DateOnly.ParseExact(r.Date, "yyyy-MM-dd", CultureInfo.InvariantCulture),
                StoreName = r.StoreName,
                Address = r.Address,
                PostCode = r.PostCode,
                StoreStart = TimeOnly.ParseExact(r.StoreStart, "HH:mm", CultureInfo.InvariantCulture),
                StoreChannel = r.StoreChannel,
                StoreFascia = r.StoreFascia,
            });

            db.Stores.AddRange(stores);
            await db.SaveChangesAsync(ct);
        }
    }
}

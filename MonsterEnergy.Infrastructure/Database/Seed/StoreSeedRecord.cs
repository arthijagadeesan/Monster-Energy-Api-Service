namespace MonsterEnergy.Infrastructure.Database.Seed
{
    internal record StoreSeedRecord(
        string StoreGuid,
        string Tdm,
        string Territory,
        string Region,
        string Date,
        string StoreName,
        string Address,
        string PostCode,
        string StoreStart,
        string StoreChannel,
        string StoreFascia);
}

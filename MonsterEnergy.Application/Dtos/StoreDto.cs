namespace MonsterEnergy.Application.Dtos
{
    public record StoreDto(
        Guid StoreGuid,
        string Tdm,
        string Territory,
        string Region,
        string Date,
        string StoreName,
        string Address,
        string PostCode,
        string StoreStart,
        string StoreChannel,
        string StoreFascia,
        bool IsFavourite);
}

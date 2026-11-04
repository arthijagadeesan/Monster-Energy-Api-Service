using MonsterEnergy.Domain.Entities;

namespace MonsterEnergy.Application.Dtos
{
    public record StoreWithFavourite(Store Store, bool IsFavourite);
}

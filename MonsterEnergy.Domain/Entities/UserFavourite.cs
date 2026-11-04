namespace MonsterEnergy.Domain.Entities
{
    public class UserFavourite
    {
        public string Username { get; set; } = string.Empty;
        public Guid StoreGuid { get; set; }
        public DateTime CreatedAt { get; set; }
        public Store? Store { get; set; }
    }
}

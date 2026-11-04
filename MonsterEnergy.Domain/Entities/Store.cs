namespace MonsterEnergy.Domain.Entities
{
    public class Store
    {
        public Guid StoreGuid { get; set; }
        public string Tdm { get; set; } = string.Empty;
        public string Territory { get; set; } = string.Empty;
        public string Region { get; set; } = string.Empty;
        public string StoreName { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string PostCode { get; set; } = string.Empty;
        public string StoreChannel { get; set; } = string.Empty;
        public string StoreFascia { get; set; } = string.Empty;
        public DateOnly Date { get; set; }
        public TimeOnly StoreStart { get; set; }
    }
}

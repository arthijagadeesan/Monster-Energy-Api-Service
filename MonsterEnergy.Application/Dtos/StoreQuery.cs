using System.ComponentModel.DataAnnotations;

namespace MonsterEnergy.Application.Dtos
{
    public class StoreQuery
    {
        [StringLength(100)]
        public string? Search { get; set; }

        [StringLength(100)]
        public string? Region { get; set; }

        [StringLength(100)]
        public string? Channel { get; set; }

        [RegularExpression("^(storeName|tdm|region|storeChannel|date|storeStart)$",
            ErrorMessage = "SortBy must be one of: storeName, tdm, region, storeChannel, date, storeStart.")]
        public string SortBy { get; set; } = "date";

        [RegularExpression("^(asc|desc)$", ErrorMessage = "SortDir must be 'asc' or 'desc'.")]
        public string SortDir { get; set; } = "asc";

        [Range(1, int.MaxValue)]
        public int Page { get; set; } = 1;

        [Range(1, 100)]
        public int PageSize { get; set; } = 10;

        public bool FavouritesOnly { get; set; }
    }
}

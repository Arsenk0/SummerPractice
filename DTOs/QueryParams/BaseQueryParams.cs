// TransportLogistics.Api/DTOs/QueryParams/BaseQueryParams.cs
using System.ComponentModel.DataAnnotations;

namespace TransportLogistics.Api.DTOs.QueryParams
{
    /// <summary>
    /// Базові параметри запиту для пагінації та сортування.
    /// </summary>
    public abstract class BaseQueryParams
    {
        // Параметри пагінації
        [Range(1, int.MaxValue, ErrorMessage = "PageNumber must be at least 1.")]
        public int PageNumber { get; set; } = 1;

        [Range(1, 100, ErrorMessage = "PageSize must be between 1 and 100.")]
        public int PageSize { get; set; } = 10;

        // Параметр сортування
        /// <summary>
        /// Поле, за яким потрібно сортувати.
        /// Наприклад: "FirstName", "LastName", "CreationDate".
        /// </summary>
        public string? SortBy { get; set; }

        /// <summary>
        /// Порядок сортування: "asc" для зростання, "desc" для спадання.
        /// </summary>
        public string SortOrder { get; set; } = "asc"; // За замовчуванням зростання

        /// <summary>
        /// Повертає зміщення для пропуску записів при пагінації.
        /// </summary>
        public int Skip => (PageNumber - 1) * PageSize;
    }
}
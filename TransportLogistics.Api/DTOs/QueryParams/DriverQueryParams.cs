// TransportLogistics.Api/DTOs/QueryParams/DriverQueryParams.cs
using System.ComponentModel.DataAnnotations;

namespace TransportLogistics.Api.DTOs.QueryParams
{
    /// <summary>
    /// Параметри запиту для фільтрування, пагінації та сортування водіїв.
    /// </summary>
    public class DriverQueryParams : BaseQueryParams
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? LicenseNumber { get; set; }
        public bool? IsAvailable { get; set; } // Для фільтрування за доступністю
    }
}
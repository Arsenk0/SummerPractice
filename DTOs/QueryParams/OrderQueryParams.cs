// TransportLogistics.Api/DTOs/QueryParams/OrderQueryParams.cs
using TransportLogistics.Api.Data.Entities; // Для OrderStatus
using System;
using System.ComponentModel.DataAnnotations;

namespace TransportLogistics.Api.DTOs.QueryParams
{
    /// <summary>
    /// Параметри запиту для фільтрування, пагінації та сортування замовлень.
    /// </summary>
    public class OrderQueryParams : BaseQueryParams
    {
        public string? OriginAddress { get; set; }
        public string? DestinationAddress { get; set; }
        public OrderStatus? Status { get; set; } // Для фільтрування за статусом
        public DateTime? CreationDateFrom { get; set; }
        public DateTime? CreationDateTo { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
    }
}
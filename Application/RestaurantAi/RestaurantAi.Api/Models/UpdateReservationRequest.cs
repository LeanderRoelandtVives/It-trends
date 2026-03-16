using System;

namespace RestaurantAi.Api.Models
{
    public class UpdateReservationRequest
    {
        public DateTime Date { get; set; }
        public string Time { get; set; } = string.Empty;
        public int PartySize { get; set; }
        public string? SpecialRequests { get; set; }
    }
}

using System;

namespace RestaurantAi.Model
{
    public class Reservation
    {
        public int Id { get; set; }
        public string OwnerId { get; set; }
        public DateTime Date { get; set; }
        public string Time { get; set; }
        public int PartySize { get; set; }
        public string? SpecialRequests { get; set; }
    }
}

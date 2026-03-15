using System.ComponentModel.DataAnnotations;

namespace RestaurantAi.Mvc.Models
{
    public class ReservationViewModel
    {
        [Required]
        [DataType(DataType.Date)]
        public DateTime Date { get; set; }

        [Required]
        public string Time { get; set; } = string.Empty;

        [Required]
        [Range(1, 20)]
        public int PartySize { get; set; }

        public string? SpecialRequests { get; set; }
    }
}

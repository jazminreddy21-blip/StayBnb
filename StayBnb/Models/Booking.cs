using System.ComponentModel.DataAnnotations;

namespace StayBnb.Models
{
    public class Booking
    {
        public int Id { get; set; }

        [Required]
        public string GuestId { get; set; } = string.Empty;

        public ApplicationUser? Guest { get; set; }

        [Required]
        public int PropertyId { get; set; }

        public Property? Property { get; set; }

        [Required]
        public DateTime CheckInDate { get; set; }

        [Required]
        public DateTime CheckOutDate { get; set; }

        [Range(1, 20)]
        public int NumberOfGuests { get; set; }

        [Range(0, 10000000)]
        public decimal TotalPrice { get; set; }

        [Required]
        public string Status { get; set; } = "Pending";

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
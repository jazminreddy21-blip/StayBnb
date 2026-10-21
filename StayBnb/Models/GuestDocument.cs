using System.ComponentModel.DataAnnotations;

namespace StayBnb.Models
{
    public class GuestDocument
    {
        public int Id { get; set; }

        [Required]
        public int BookingId { get; set; }

        public Booking? Booking { get; set; }

        [Required]
        public string GuestId { get; set; } = string.Empty;

        public ApplicationUser? Guest { get; set; }

        [Required]
        public string DocumentType { get; set; } = string.Empty;

        [Required]
        public string FilePath { get; set; } = string.Empty;

        [Required]
        public string Status { get; set; } = "Pending";

        public string? VerificationNotes { get; set; }

        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

        public DateTime? VerifiedAt { get; set; }
    }
}
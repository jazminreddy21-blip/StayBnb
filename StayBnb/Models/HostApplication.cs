
using System.ComponentModel.DataAnnotations;

namespace StayBnb.Models
{
    public class HostApplication
    {
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        public ApplicationUser? User { get; set; }

        [Required]
        public int PropertyId { get; set; }

        public Property? Property { get; set; }

        [Required]
        public string Status { get; set; } = "Pending";

        public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;

        public DateTime? ReviewedAt { get; set; }

        public string? ReviewNotes { get; set; }
    }
}
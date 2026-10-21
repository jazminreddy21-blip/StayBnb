using System.ComponentModel.DataAnnotations;

namespace StayBnb.Models
{
    public class CheckInProcess
    {
        public int Id { get; set; }

        [Required]
        public int PropertyId { get; set; }

        public Property? Property { get; set; }

        [Required]
        public string Instructions { get; set; } = string.Empty;

        [Required]
        public string RequiredDocuments { get; set; } = string.Empty;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}

using System.ComponentModel.DataAnnotations;

namespace StayBnb.Models
{
    public class PropertyImage
    {
        public int Id { get; set; }

        [Required]
        public string ImageUrl { get; set; } = string.Empty;

        public bool IsPrimary { get; set; } = false;

        // Foreign key
        public int PropertyId { get; set; }

        // Navigation property
        public Property? Property { get; set; }
    }
}
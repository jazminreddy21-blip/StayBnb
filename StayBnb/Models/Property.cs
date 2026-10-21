
using System.ComponentModel.DataAnnotations;

namespace StayBnb.Models
{
    public class Property
    {
        public int Id { get; set; }

        [Required]
        [StringLength(150)]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Description { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string City { get; set; } = string.Empty;

        [Required]
        [StringLength(250)]
        public string Address { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string PropertyType { get; set; } = string.Empty;

        [Range(1, 1000000)]
        public decimal PricePerNight { get; set; }

        [Range(0, 1000000)]
        public decimal CleaningFee { get; set; }

        [Range(0, 1000000)]
        public decimal ServiceFee { get; set; }

        public bool IsActive { get; set; } = false;

        // The host who owns this property
        [Required]
        public string HostId { get; set; } = string.Empty;

        public ApplicationUser? Host { get; set; }

        // Images belonging to this property
        public ICollection<PropertyImage> Images { get; set; }
            = new List<PropertyImage>();
    }
}
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Propertify.Web.Models
{
    public class Property
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(200)]
        public required string Name { get; set; }

        [Required]
        [MaxLength(100)]
        public required string Type { get; set; }

        [Required]
        [MaxLength(500)]
        public required string Location { get; set; }

        public int TotalUnits { get; set; }

        [MaxLength(500)]
        public string? ImagePath { get; set; }

        [MaxLength(500)]
        public string? ImageUrl { get; set; }

        [NotMapped]
        public IFormFile? ImageFile { get; set; }

        public double? Latitude { get; set; }
        public double? Longitude { get; set; }

        public ICollection<Unit> Units { get; set; } = new List<Unit>();

        
    }
}

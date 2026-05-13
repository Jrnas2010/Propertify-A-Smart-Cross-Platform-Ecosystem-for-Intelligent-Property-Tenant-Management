using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Propertify.Web.Models
{
    public class Unit
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string UnitNumber { get; set; } = string.Empty;

        public int FloorNumber { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal RentAmount { get; set; }

        public double Area { get; set; }

        public bool IsOccupied { get; set; } = false;

        [Required]
        [MaxLength(50)]
        public string Status { get; set; } = "Vacant";

        [MaxLength(100)]
        public string? ElectricityMeter { get; set; }

        [MaxLength(100)]
        public string? WaterMeter { get; set; }

        public int Bedrooms { get; set; }
        public int Bathrooms { get; set; }
        public int Kitchens { get; set; }
        public int LivingRooms { get; set; }
        public int Majlis { get; set; }

        public string? UnitImages { get; set; }

        [MaxLength(500)]
        public string? VideoPath { get; set; }

        public int PropertyId { get; set; }

        [ForeignKey("PropertyId")]
        public Property? Property { get; set; }

        [NotMapped]
        public string? ThumbnailUrl => !string.IsNullOrEmpty(UnitImages)
            ? UnitImages.Split(',')[0]
            : "/images/default-unit.png";
    }
}

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Propertify.Web.Models
{
    public class MaintenanceRequest
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(255)]
        public string Title { get; set; } = "Maintenance Request";

        public string? Description { get; set; }

        [Range(0, 999999)]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Cost { get; set; }

        [Required]
        [MaxLength(50)]
        public string Status { get; set; } = "Pending";

        [Required]
        [MaxLength(50)]
        public string Priority { get; set; } = "Normal";

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [MaxLength(500)]
        public string? ImagePath { get; set; }

        public int PropertyId { get; set; }

        [ForeignKey("PropertyId")]
        public Property? Property { get; set; }

        [MaxLength(200)]
        public string? PropertyName { get; set; }

        public int UnitId { get; set; }

        [ForeignKey("UnitId")]
        public Unit? Unit { get; set; }
    }
}

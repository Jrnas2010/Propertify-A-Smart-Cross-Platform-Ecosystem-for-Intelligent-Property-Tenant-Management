using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Propertify.Web.Models
{
    public class UtilityBill
    {
        [Key]
        public int BillId { get; set; }

        [Required]
        [MaxLength(50)]
        public required string ServiceType { get; set; }

        [Required]
        [Range(0, 9999999.99)]
        [Column(TypeName = "decimal(18,2)")]
        public decimal PreviousReading { get; set; }

        [Required]
        [Range(0, 9999999.99)]
        [Column(TypeName = "decimal(18,2)")]
        public decimal CurrentReading { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalAmount { get; set; }

        public DateTime IssueDate { get; set; } = DateTime.Now;

        public int UnitId { get; set; }

        [ForeignKey("UnitId")]
        public virtual Unit? Unit { get; set; }

        public int TenantId { get; set; }

        [ForeignKey("TenantId")]
        public virtual Tenant? Tenant { get; set; }
    }
}

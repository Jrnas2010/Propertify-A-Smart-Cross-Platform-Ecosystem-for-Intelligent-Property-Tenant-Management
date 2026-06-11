using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Propertify.Web.Models
{
    public class Contract
    {
        [Key]
        public int Id { get; set; }

        public int TenantId { get; set; }
        public Tenant? Tenant { get; set; }

        public int UnitId { get; set; }
        public Unit? Unit { get; set; }

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        /// <summary>Total contract value (used for income calculations and revenue charts).</summary>
        [Column(TypeName = "decimal(18,2)")]
        public decimal RentAmount { get; set; }

        /// <summary>Monthly rent amount shown to the tenant in the mobile app.</summary>
        [Column(TypeName = "decimal(18,2)")]
        public decimal MonthlyRent { get; set; }

        [Required]
        [MaxLength(50)]
        public string Status { get; set; } = "Active";
    }
}

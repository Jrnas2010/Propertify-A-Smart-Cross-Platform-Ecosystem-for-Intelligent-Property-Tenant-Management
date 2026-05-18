using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Propertify.Web.Models
{
    public class User
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(200)]
        public required string FullName { get; set; }

        [Required]
        [EmailAddress]
        [MaxLength(254)]
        public required string Email { get; set; }

        [Required]
        [MaxLength(500)]
        public required string Password { get; set; }

        [Required]
        [MaxLength(20)]
        public required string Role { get; set; }

        [MaxLength(20)]
        public string Status { get; set; } = "Active";

        [MaxLength(1000)]
        public string? Permissions { get; set; }

        public bool IsSystemAdmin { get; set; } = false;

        public int? TenantId { get; set; }

        [ForeignKey("TenantId")]
        public Tenant? Tenant { get; set; }
    }
}

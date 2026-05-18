using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Propertify.Web.Models
{
    public class ActivityLog
    {
        [Key]
        public int Id { get; set; }

        public int UserId { get; set; }

        [Required, MaxLength(200)]
        public string UserName { get; set; } = string.Empty;

        [Required, MaxLength(100)]
        public string Action { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Details { get; set; }

        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        [MaxLength(50)]
        public string Platform { get; set; } = "Mobile";

        [ForeignKey("UserId")]
        public User? User { get; set; }
    }
}

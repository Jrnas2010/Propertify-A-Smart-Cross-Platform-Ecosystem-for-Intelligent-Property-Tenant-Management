using System.ComponentModel.DataAnnotations;

namespace Propertify.Web.Models
{
    public class BookingRequest
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(255)]
        public string VisitorName { get; set; } = string.Empty;

        [Required]
        [Phone]
        [MaxLength(20)]
        public string Phone { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [MaxLength(255)]
        public string Email { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string RequestType { get; set; } = string.Empty;

        [MaxLength(50)]
        public string? UnitNumber { get; set; }

        [MaxLength(1000)]
        public string Notes { get; set; } = string.Empty;

        public DateTime SubmittedAt { get; set; } = DateTime.Now;
        public bool IsRead { get; set; } = false;
    }
}

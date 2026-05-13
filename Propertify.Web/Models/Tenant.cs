using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Propertify.Web.Models
{
    public class Tenant
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string FirstNameAr { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? SecondNameAr { get; set; }

        [MaxLength(100)]
        public string? ThirdNameAr { get; set; }

        [Required]
        [MaxLength(100)]
        public string LastNameAr { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string FirstNameEn { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? SecondNameEn { get; set; }

        [MaxLength(100)]
        public string? ThirdNameEn { get; set; }

        [Required]
        [MaxLength(100)]
        public string LastNameEn { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string IdNumber { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? IdDocumentPath { get; set; }

        [Required]
        [MaxLength(20)]
        public string Phone { get; set; } = string.Empty;

        [MaxLength(100)]
        [EmailAddress]
        public string? Email { get; set; }

        [MaxLength(50)]
        public string? Nationality { get; set; }

        public DateTime LeaseStartDate { get; set; } = DateTime.Now;
        public DateTime LeaseEndDate { get; set; } = DateTime.Now.AddYears(1);

        public bool IsArchived { get; set; } = false;

        public int UnitId { get; set; }

        [ForeignKey("UnitId")]
        public Unit? Unit { get; set; }

        [NotMapped]
        public string FullNameAr => string.Join(" ", new[] { FirstNameAr, SecondNameAr, ThirdNameAr, LastNameAr }
            .Where(n => !string.IsNullOrWhiteSpace(n)));

        [NotMapped]
        public string FullNameEn => string.Join(" ", new[] { FirstNameEn, SecondNameEn, ThirdNameEn, LastNameEn }
            .Where(n => !string.IsNullOrWhiteSpace(n)));
    }
}

using System.ComponentModel.DataAnnotations;

namespace Propertify.Web.Models
{
    public class SystemSetting
    {
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(100)]
        public string Key { get; set; } = string.Empty;

        [MaxLength(500)]
        public string Value { get; set; } = string.Empty;
    }
}

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Propertify.Web.Models
{
    public class SystemMessage
    {
        [Key]
        public int MessageId { get; set; }

        [MaxLength(200)]
        public string? Subject { get; set; }

        public string? Content { get; set; }

        public DateTime SentAt { get; set; } = DateTime.Now;

        [MaxLength(50)]
        public string? Target { get; set; }

        public bool IsBroadcast { get; set; } = true;
    }
}

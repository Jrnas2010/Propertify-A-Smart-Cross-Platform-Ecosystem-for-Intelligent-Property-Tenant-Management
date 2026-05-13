namespace Propertify.Web.Models
{
    public class MaintenanceRequestDto
    {
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int UnitId { get; set; }
        public string? Priority { get; set; }
        public IFormFile? ImageFile { get; set; }
    }
}

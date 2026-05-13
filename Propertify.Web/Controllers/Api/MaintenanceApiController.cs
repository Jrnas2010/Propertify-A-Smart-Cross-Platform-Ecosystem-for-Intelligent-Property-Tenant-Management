using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Propertify.Web.Data;
using Propertify.Web.Models;

namespace Propertify.Web.Controllers.Api
{
    [Route("api/maintenance")]
    [ApiController]
    [Authorize]
    public class MaintenanceApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;

        public MaintenanceApiController(ApplicationDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        [HttpPost("submit")]
        public async Task<IActionResult> SubmitRequest([FromForm] MaintenanceRequestDto dto)
        {
            var unit = await _context.Units.FindAsync(dto.UnitId);
            if (unit == null)
                return BadRequest(new { message = "Unit not found." });

            string? imagePath = null;
            if (dto.ImageFile != null)
            {
                var allowedMime = new[] { "image/jpeg", "image/png", "image/webp" };
                if (!allowedMime.Contains(dto.ImageFile.ContentType.ToLower()))
                    return BadRequest(new { message = "Only JPEG, PNG, or WebP images are allowed." });

                if (dto.ImageFile.Length > 5 * 1024 * 1024)
                    return BadRequest(new { message = "Image must be under 5 MB." });

                string ext = Path.GetExtension(dto.ImageFile.FileName).ToLower();
                string fileName = Guid.NewGuid() + ext;
                string uploadDir = Path.Combine(_env.WebRootPath, "uploads", "maintenance");
                Directory.CreateDirectory(uploadDir);

                string filePath = Path.Combine(uploadDir, fileName);
                try
                {
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await dto.ImageFile.CopyToAsync(stream);
                    }
                }
                catch
                {
                    return StatusCode(500, new { message = "File could not be saved. Please try again." });
                }
                imagePath = "/uploads/maintenance/" + fileName;
            }

            var request = new MaintenanceRequest
            {
                Title = dto.Title,
                Description = dto.Description,
                UnitId = dto.UnitId,
                PropertyId = unit.PropertyId,
                Priority = dto.Priority ?? "Normal",
                Status = "Pending",
                ImagePath = imagePath,
                CreatedAt = DateTime.Now
            };

            _context.MaintenanceRequests.Add(request);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Request submitted successfully!" });
        }
    }
}

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Propertify.Web.Data;
using Propertify.Web.Models;

namespace Propertify.Web.Controllers
{
    [Authorize(Roles = "Owner")]
    public class SettingsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public SettingsController(ApplicationDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        public IActionResult Index()
        {
            // جلب إعدادات النظام الحالية (مثل اسم الشركة، الشعار، العملة)
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateSystemSettings(string siteName, IFormFile? LogoFile)
        {
            if (LogoFile != null)
            {
                var allowedMime = new[] { "image/jpeg", "image/png", "image/webp", "image/svg+xml" };
                if (!allowedMime.Contains(LogoFile.ContentType.ToLower()))
                {
                    TempData["Error"] = "Logo must be a JPEG, PNG, WebP, or SVG image.";
                    return RedirectToAction(nameof(Index));
                }

                if (LogoFile.Length > 2 * 1024 * 1024)
                {
                    TempData["Error"] = "Logo file must be under 2 MB.";
                    return RedirectToAction(nameof(Index));
                }

                try
                {
                    string uploadFolder = Path.Combine(_environment.WebRootPath, "uploads", "branding");
                    Directory.CreateDirectory(uploadFolder);

                    string ext = Path.GetExtension(LogoFile.FileName).ToLower();
                    string filePath = Path.Combine(uploadFolder, "logo" + ext);

                    using var stream = new FileStream(filePath, FileMode.Create);
                    await LogoFile.CopyToAsync(stream);
                }
                catch
                {
                    TempData["Error"] = "Could not save logo file. Please try again.";
                    return RedirectToAction(nameof(Index));
                }
            }

            TempData["Success"] = "System settings updated successfully.";
            return RedirectToAction(nameof(Index));
        }
    }
}
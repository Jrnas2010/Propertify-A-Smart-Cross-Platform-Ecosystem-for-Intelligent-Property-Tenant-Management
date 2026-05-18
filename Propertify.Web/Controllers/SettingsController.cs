using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Propertify.Web.Data;
using Propertify.Web.Models;
using Propertify.Web.Services;

namespace Propertify.Web.Controllers
{
    [Authorize(Roles = "Owner")]
    public class SettingsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;
        private readonly SystemSettingService _settings;

        public SettingsController(ApplicationDbContext context,
                                  IWebHostEnvironment environment,
                                  SystemSettingService settings)
        {
            _context  = context;
            _environment = environment;
            _settings = settings;
        }

        public async Task<IActionResult> Index()
        {
            var allSettings = await _context.SystemSettings.ToListAsync();
            ViewBag.SettingsDict = allSettings.ToDictionary(s => s.Key, s => s.Value);
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveAppearance(string sidebarColor, string primaryColor,
                                                         string accentColor, string fontFamily)
        {
            var keys = new Dictionary<string, string>
            {
                ["SidebarColor"] = sidebarColor ?? "#0f172a",
                ["PrimaryColor"] = primaryColor ?? "#1e3a5f",
                ["AccentColor"]  = accentColor  ?? "#3b82f6",
                ["FontFamily"]   = fontFamily   ?? "Inter"
            };

            foreach (var kv in keys)
            {
                var existing = await _context.SystemSettings.FirstOrDefaultAsync(s => s.Key == kv.Key);
                if (existing != null) existing.Value = kv.Value;
                else _context.SystemSettings.Add(new SystemSetting { Key = kv.Key, Value = kv.Value });
            }
            await _context.SaveChangesAsync();
            _settings.InvalidateCache();

            TempData["Success"] = "Appearance settings saved successfully.";
            return RedirectToAction(nameof(Index), new { tab = "appearance" });
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

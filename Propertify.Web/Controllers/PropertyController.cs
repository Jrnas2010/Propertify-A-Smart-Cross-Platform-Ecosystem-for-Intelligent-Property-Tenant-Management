using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Propertify.Web.Data;
using Propertify.Web.Models;

namespace Propertify.Web.Controllers
{
    [Authorize(Roles = "Owner")]
    public class PropertyController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public PropertyController(ApplicationDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        public async Task<IActionResult> BrowseAvailableUnits()
{
    var availableUnits = await _context.Units
        .Include(u => u.Property) // ربط الوحدة بالبناية
        .Where(u => u.Status == "Vacant")
        .ToListAsync();

    return View(availableUnits);
}

        public async Task<IActionResult> Index(string searchTerm)
        {
            if (!string.IsNullOrEmpty(searchTerm) && searchTerm.Length > 100)
                searchTerm = searchTerm[..100];

            ViewBag.SearchTerm = searchTerm;

            var query = _context.Properties
                .Include(p => p.Units)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(p =>
                    p.Name.Contains(searchTerm) ||
                    p.Location.Contains(searchTerm));
            }

            return View(await query.ToListAsync());
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        public async Task<IActionResult> Details(int id)
        {
            var property = await _context.Properties
                .Include(p => p.Units)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (property == null) return NotFound();

            return View(property);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Property property, IFormFile ImageFile, List<IFormFileCollection> UnitImageFiles, List<IFormFile> VideoFiles)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    if (property.ImageFile != null)
                    {
                        property.ImageUrl = await UploadFile(property.ImageFile, "properties");
                    }

                    if (property.Units != null && property.Units.Count > 0)
                    {
                        for (int i = 0; i < property.Units.Count; i++)
                        {
                            var unit = property.Units.ElementAt(i);

                            var files = Request.Form.Files.GetFiles($"Units[{i}].UnitImageFiles");
                            List<string> imagePaths = new List<string>();
                            foreach (var file in files)
                            {
                                imagePaths.Add(await UploadFile(file, "units/images"));
                            }
                            unit.UnitImages = string.Join(",", imagePaths);

                            var videoFile = Request.Form.Files.GetFile($"Units[{i}].VideoFile");
                            if (videoFile != null)
                            {
                                unit.VideoPath = await UploadFile(videoFile, "units/videos");
                            }

                            unit.Status = "Vacant";
                        }
                    }

                    _context.Add(property);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (InvalidOperationException ex)
                {
                    ModelState.AddModelError(string.Empty, ex.Message);
                }
            }
            return View("Index", await _context.Properties.ToListAsync());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var property = await _context.Properties.Include(p => p.Units).FirstOrDefaultAsync(x => x.Id == id);
            if (property == null) return NotFound();

            try
            {
                _context.Properties.Remove(property);
                await _context.SaveChangesAsync();
            }
            catch (Microsoft.EntityFrameworkCore.DbUpdateException)
            {
                TempData["Error"] = "Cannot delete this property because it has linked records (tenants, contracts, or maintenance requests). Archive or remove them first.";
            }
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int id)
        {
            var property = await _context.Properties
                .Include(p => p.Units)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (property == null) return NotFound();

            return View(property);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Property property, IFormFile? ImageFile)
        {
            if (id != property.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    if (ImageFile != null)
                    {
                        // Use ImageUrl consistently (same field as Create)
                        property.ImageUrl = await UploadFile(ImageFile, "properties");
                    }

                    if (property.Units != null)
                    {
                        for (int i = 0; i < property.Units.Count; i++)
                        {
                            var unit = property.Units.ElementAt(i);

                            var files = Request.Form.Files.GetFiles($"Units[{i}].UnitImageFiles");
                            if (files.Any())
                            {
                                List<string> imagePaths = new List<string>();
                                foreach (var file in files)
                                {
                                    imagePaths.Add(await UploadFile(file, "units/images"));
                                }
                                unit.UnitImages = string.Join(",", imagePaths);
                            }

                            var videoFile = Request.Form.Files.GetFile($"Units[{i}].VideoFile");
                            if (videoFile != null)
                            {
                                unit.VideoPath = await UploadFile(videoFile, "units/videos");
                            }
                        }
                    }

                    _context.Update(property);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PropertyExists(property.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(property);
        }

        private bool PropertyExists(int id) => _context.Properties.Any(e => e.Id == id);

        private static readonly string[] AllowedImageMime = ["image/jpeg", "image/png", "image/webp"];
        private static readonly string[] AllowedVideoMime = ["video/mp4", "video/quicktime", "video/webm"];
        private const long MaxImageBytes = 10 * 1024 * 1024; // 10 MB
        private const long MaxVideoBytes = 100 * 1024 * 1024; // 100 MB

        private async Task<string> UploadFile(IFormFile file, string folder)
        {
            bool isVideo = folder.Contains("video");
            var allowed = isVideo ? AllowedVideoMime : AllowedImageMime;
            long maxSize = isVideo ? MaxVideoBytes : MaxImageBytes;

            if (!allowed.Contains(file.ContentType.ToLower()))
                throw new InvalidOperationException($"File type '{file.ContentType}' is not allowed.");

            if (file.Length > maxSize)
                throw new InvalidOperationException($"File exceeds the maximum allowed size of {maxSize / 1024 / 1024} MB.");

            string folderPath = Path.Combine(_environment.WebRootPath, "uploads", folder);
            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);

            string ext = Path.GetExtension(file.FileName).ToLower();
            string fileName = Guid.NewGuid().ToString() + ext;
            string filePath = Path.Combine(folderPath, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return $"/uploads/{folder}/{fileName}";
        }
    }
}

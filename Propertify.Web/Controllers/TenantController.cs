using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Propertify.Web.Data;
using Propertify.Web.Models;

namespace Propertify.Web.Controllers
{
    [Authorize(Roles = "Owner")] 
    public class TenantController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public TenantController(ApplicationDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        // 1. عرض قائمة المستأجرين مع البحث والأرشفة
        public async Task<IActionResult> Index(string searchTerm, bool showArchived = false)
        {
            if (!string.IsNullOrEmpty(searchTerm) && searchTerm.Length > 100)
                searchTerm = searchTerm[..100];

            ViewBag.SearchTerm = searchTerm;
            ViewBag.ShowingArchived = showArchived;

            var query = _context.Tenants
                .Include(t => t.Unit)
                .Where(t => t.IsArchived == showArchived)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = ApplyFilters(query, searchTerm);
            }

            return View(await query.ToListAsync());
        }

        public async Task<IActionResult> Create()
        {
            var vacantUnits = await _context.Units.Where(u => !u.IsOccupied).ToListAsync();
            ViewBag.UnitId = new SelectList(vacantUnits, "Id", "UnitNumber");
            return View();
        }

        // 3. معالجة إضافة مستأجر جديد
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Tenant tenant, IFormFile? IdCardImage)
        {
            if (ModelState.IsValid)
            {
                // رفع صورة الهوية باستخدام الدالة المساعدة
                if (IdCardImage != null)
                {
                    tenant.IdDocumentPath = await UploadFile(IdCardImage, "tenants/docs");
                }

                tenant.IsArchived = false;
                _context.Add(tenant);

                // تحديث حالة الوحدة المرتبطة لتصبح "مشغولة"
                var unit = await _context.Units.FindAsync(tenant.UnitId);
                if (unit != null)
                {
                    unit.IsOccupied = true;
                    unit.Status = "Occupied";
                }

                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewBag.UnitId = new SelectList(await _context.Units.Where(u => !u.IsOccupied).ToListAsync(), "Id", "UnitNumber", tenant.UnitId);
            return View(tenant);
        }

        // 4. أرشفة المستأجر عند انتهاء العقد وإخلاء الوحدة
        [HttpPost]
        public async Task<IActionResult> Archive(int id)
        {
            var tenant = await _context.Tenants.FindAsync(id);
            if (tenant != null)
            {
                tenant.IsArchived = true;
                
                // تحويل الوحدة لتصبح شاغرة مرة أخرى بعد أرشفة المستأجر
                var unit = await _context.Units.FindAsync(tenant.UnitId);
                if (unit != null)
                {
                    unit.IsOccupied = false;
                    unit.Status = "Vacant";
                }

                _context.Update(tenant);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        // 4.5. عرض تفاصيل المستأجر
        public async Task<IActionResult> Details(int id)
        {
            var tenant = await _context.Tenants
                .Include(t => t.Unit)
                    .ThenInclude(u => u.Property)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (tenant == null)
            {
                return NotFound();
            }

            return View(tenant);
        }

        // 4.6. عرض صفحة تعديل المستأجر
        public async Task<IActionResult> Edit(int id)
        {
            var tenant = await _context.Tenants.FindAsync(id);
            if (tenant == null)
            {
                return NotFound();
            }
            var vacantUnits = await _context.Units.Where(u => !u.IsOccupied || u.Id == tenant.UnitId).ToListAsync();
            ViewBag.UnitId = new SelectList(vacantUnits, "Id", "UnitNumber", tenant.UnitId);
            return View(tenant);
        }

        // 4.7. معالجة تعديل المستأجر
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Tenant tenant, IFormFile? IdCardImage)
        {
            if (id != tenant.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var existingTenant = await _context.Tenants.AsNoTracking().FirstOrDefaultAsync(t => t.Id == id);
                    if (existingTenant == null) return NotFound();

                    if (IdCardImage != null)
                    {
                        tenant.IdDocumentPath = await UploadFile(IdCardImage, "tenants/docs");
                    }
                    else
                    {
                        tenant.IdDocumentPath = existingTenant.IdDocumentPath;
                    }

                    // Handling unit changes
                    if (existingTenant.UnitId != tenant.UnitId)
                    {
                        // Free old unit
                        var oldUnit = await _context.Units.FindAsync(existingTenant.UnitId);
                        if (oldUnit != null)
                        {
                            oldUnit.IsOccupied = false;
                            oldUnit.Status = "Vacant";
                        }

                        // Occupy new unit
                        var newUnit = await _context.Units.FindAsync(tenant.UnitId);
                        if (newUnit != null)
                        {
                            newUnit.IsOccupied = true;
                            newUnit.Status = "Occupied";
                        }
                    }

                    _context.Update(tenant);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TenantExists(tenant.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewBag.UnitId = new SelectList(await _context.Units.Where(u => !u.IsOccupied || u.Id == tenant.UnitId).ToListAsync(), "Id", "UnitNumber", tenant.UnitId);
            return View(tenant);
        }

        private bool TenantExists(int id)
        {
            return _context.Tenants.Any(e => e.Id == id);
        }

        // 5. حذف نهائي للمستأجر
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var tenant = await _context.Tenants.FindAsync(id);
            if (tenant != null)
            {
                _context.Tenants.Remove(tenant);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        // --- دالت مساعدة (Helper Methods) ---

        private IQueryable<Tenant> ApplyFilters(IQueryable<Tenant> query, string searchTerm)
        {
            return query.Where(t => 
                   t.FirstNameAr.Contains(searchTerm) || t.LastNameAr.Contains(searchTerm)
                || t.FirstNameEn.Contains(searchTerm) || t.LastNameEn.Contains(searchTerm)
                || t.IdNumber.Contains(searchTerm)
                || t.Phone.Contains(searchTerm)
                || (t.Unit != null && t.Unit.UnitNumber.Contains(searchTerm)));
        }

        private async Task<string> UploadFile(IFormFile file, string subFolder)
        {
            string uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", subFolder);
            if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

            string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
            string filePath = Path.Combine(uploadsFolder, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }
            return $"/uploads/{subFolder}/{fileName}";
        }
    }
}
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Propertify.Web.Data;
using Propertify.Web.Helpers;
using Propertify.Web.Models;

namespace Propertify.Web.Controllers
{
    /// <summary>Manages tenant records: listing, creation, editing, archiving, and mobile-account provisioning.</summary>
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

        /// <summary>Lists tenants filtered by search term and archive status. Passes a tenantId→email map so the view can show which tenants already have mobile accounts.</summary>
        public async Task<IActionResult> Index(string searchTerm, bool showArchived = false)
        {
            if (!string.IsNullOrEmpty(searchTerm) && searchTerm.Length > 100)
                searchTerm = searchTerm[..100];

            ViewBag.SearchTerm = searchTerm;
            ViewBag.ShowingArchived = showArchived;

            var query = _context.Tenants
                .Include(t => t.Unit)
                    .ThenInclude(u => u!.Property)
                .Where(t => t.IsArchived == showArchived)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchTerm))
                query = ApplyFilters(query, searchTerm);

            var tenants = await query.ToListAsync();

            // Map tenantId → account email for tenants that already have a mobile account
            var tenantIds = tenants.Select(t => t.Id).ToList();
            ViewBag.TenantAccounts = await _context.Users
                .Where(u => u.TenantId != null && tenantIds.Contains(u.TenantId.Value))
                .ToDictionaryAsync(u => u.TenantId!.Value, u => u.Email);

            return View(tenants);
        }

        /// <summary>Displays the "Add Tenant" form, populating the unit dropdown with vacant units only.</summary>
        public async Task<IActionResult> Create()
        {
            var vacantUnits = await _context.Units.Where(u => !u.IsOccupied).ToListAsync();
            ViewBag.UnitId = new SelectList(vacantUnits, "Id", "UnitNumber");
            return View();
        }

        /// <summary>Saves a new tenant, uploads the ID-card image if provided, and marks the linked unit as occupied.</summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Tenant tenant, IFormFile? IdCardImage)
        {
            if (ModelState.IsValid)
            {
                if (IdCardImage != null)
                {
                    try { tenant.IdDocumentPath = await UploadFile(IdCardImage, "tenants/docs"); }
                    catch (InvalidOperationException ex)
                    {
                        ModelState.AddModelError(string.Empty, ex.Message);
                        ViewBag.UnitId = new SelectList(await _context.Units.Where(u => !u.IsOccupied).ToListAsync(), "Id", "UnitNumber", tenant.UnitId);
                        return View(tenant);
                    }
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

        /// <summary>Flags the tenant as archived and releases the linked unit back to "Vacant".</summary>
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

        /// <summary>Returns the detail view for a single tenant, including their unit and property.</summary>
        public async Task<IActionResult> Details(int id)
        {
            var tenant = await _context.Tenants
                .Include(t => t.Unit)
                    .ThenInclude(u => u!.Property)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (tenant == null)
            {
                return NotFound();
            }

            return View(tenant);
        }

        /// <summary>Displays the edit form, including the tenant's currently assigned unit in the dropdown.</summary>
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

        /// <summary>Saves tenant edits; if the unit changed, frees the old unit and occupies the new one.</summary>
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
                        try { tenant.IdDocumentPath = await UploadFile(IdCardImage, "tenants/docs"); }
                        catch (InvalidOperationException ex)
                        {
                            ModelState.AddModelError(string.Empty, ex.Message);
                            ViewBag.UnitId = new SelectList(await _context.Units.Where(u => !u.IsOccupied || u.Id == tenant.UnitId).ToListAsync(), "Id", "UnitNumber", tenant.UnitId);
                            return View(tenant);
                        }
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

        /// <summary>
        /// Provisions a mobile-app login for a tenant: validates email uniqueness, hashes the password,
        /// and creates a User record with Role="Tenant" linked to the tenant's ID.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateMobileAccount(int tenantId, string email, string password, string? permissions)
        {
            var tenant = await _context.Tenants.FindAsync(tenantId);
            if (tenant == null)
            {
                TempData["Error"] = "Tenant not found.";
                return RedirectToAction(nameof(Index));
            }

            if (_context.Users.Any(u => u.Email == email.Trim().ToLower()))
            {
                TempData["Error"] = "A user with this email already exists.";
                return RedirectToAction(nameof(Index));
            }

            var existing = await _context.Users.FirstOrDefaultAsync(u => u.TenantId == tenantId);
            if (existing != null)
            {
                TempData["Error"] = $"Tenant already has a mobile account ({existing.Email}).";
                return RedirectToAction(nameof(Index));
            }

            _context.Users.Add(new User
            {
                FullName    = tenant.FullNameEn,
                Email       = email.Trim().ToLower(),
                Password    = PasswordHelper.Hash(password),
                Role        = "Tenant",
                Status      = "Active",
                Permissions = string.IsNullOrWhiteSpace(permissions) ? "Contracts,Invoices,Maintenance" : permissions,
                TenantId    = tenantId
            });
            await _context.SaveChangesAsync();

            TempData["Success"] = $"Mobile account created for {tenant.FullNameEn}.";
            return RedirectToAction(nameof(Index));
        }

        /// <summary>Permanently deletes a tenant record from the database.</summary>
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

        /// <summary>Filters tenants by name (Arabic/English), national ID, phone, or unit number.</summary>
        private IQueryable<Tenant> ApplyFilters(IQueryable<Tenant> query, string searchTerm)
        {
            return query.Where(t => 
                   t.FirstNameAr.Contains(searchTerm) || t.LastNameAr.Contains(searchTerm)
                || t.FirstNameEn.Contains(searchTerm) || t.LastNameEn.Contains(searchTerm)
                || t.IdNumber.Contains(searchTerm)
                || t.Phone.Contains(searchTerm)
                || (t.Unit != null && t.Unit.UnitNumber.Contains(searchTerm)));
        }

        private static readonly string[] AllowedDocMime = ["image/jpeg", "image/png", "image/webp", "application/pdf"];
        private const long MaxDocBytes = 10 * 1024 * 1024; // 10 MB

        /// <summary>Saves an uploaded file to <c>wwwroot/uploads/{subFolder}</c> with a GUID filename and returns the public path.</summary>
        private async Task<string> UploadFile(IFormFile file, string subFolder)
        {
            if (!AllowedDocMime.Contains(file.ContentType.ToLower()))
                throw new InvalidOperationException($"File type '{file.ContentType}' is not allowed. Accepted: JPEG, PNG, WebP, PDF.");

            if (file.Length > MaxDocBytes)
                throw new InvalidOperationException($"File exceeds the maximum allowed size of {MaxDocBytes / 1024 / 1024} MB.");

            string uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", subFolder);
            if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

            string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName).ToLower();
            string filePath = Path.Combine(uploadsFolder, fileName);

            await using var stream = new FileStream(filePath, FileMode.Create);
            await file.CopyToAsync(stream);
            return $"/uploads/{subFolder}/{fileName}";
        }
    }
}
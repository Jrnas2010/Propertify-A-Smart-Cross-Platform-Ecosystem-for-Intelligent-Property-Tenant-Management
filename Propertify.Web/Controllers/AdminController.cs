using ClosedXML.Excel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Propertify.Web.Data;
using Propertify.Web.Helpers;
using Propertify.Web.Models;

namespace Propertify.Web.Controllers
{
    [Authorize(Roles = "Owner")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        public AdminController(ApplicationDbContext context) => _context = context;

        private bool CurrentUserIsSysAdmin =>
            User.FindFirst("IsSystemAdmin")?.Value == "true";

        public async Task<IActionResult> Index(string? search, string? roleFilter, string? statusFilter)
        {
            ViewBag.IsSystemAdmin = CurrentUserIsSysAdmin;
            var query = _context.Users.Include(u => u.Tenant).AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(u => u.FullName.Contains(search) || u.Email.Contains(search));

            if (!string.IsNullOrWhiteSpace(roleFilter))
                query = query.Where(u => u.Role == roleFilter);

            if (!string.IsNullOrWhiteSpace(statusFilter))
                query = query.Where(u => u.Status == statusFilter);

            ViewBag.Search       = search;
            ViewBag.RoleFilter   = roleFilter;
            ViewBag.StatusFilter = statusFilter;
            ViewBag.Properties   = await _context.Properties.OrderBy(p => p.Name).ToListAsync();

            return View(await query.OrderBy(u => u.FullName).ToListAsync());
        }

        // ── AJAX: units for a property ─────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> GetUnitsByProperty(int propertyId)
        {
            var units = await _context.Units
                .Where(u => u.PropertyId == propertyId)
                .OrderBy(u => u.UnitNumber)
                .Select(u => new { u.Id, u.UnitNumber, u.IsOccupied })
                .ToListAsync();
            return Json(units);
        }

        // ── AJAX: tenant linked to a unit ──────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> GetTenantByUnit(int unitId)
        {
            var tenant = await _context.Tenants
                .Where(t => t.UnitId == unitId && !t.IsArchived)
                .Select(t => new { t.Id, t.FullNameEn, t.Email })
                .FirstOrDefaultAsync();
            return Json(tenant);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(string fullName, string email, string password,
            string role, string status, string? permissions, int? tenantId)
        {
            // Only system admins may create Owner accounts
            if (role == "Owner" && !CurrentUserIsSysAdmin)
            {
                TempData["Error"] = "Only the system administrator can create Owner accounts.";
                return RedirectToAction(nameof(Index));
            }

            if (_context.Users.Any(u => u.Email == email))
            {
                TempData["Error"] = "A user with this email already exists.";
                return RedirectToAction(nameof(Index));
            }

            _context.Users.Add(new User
            {
                FullName    = fullName.Trim(),
                Email       = email.Trim(),
                Password    = PasswordHelper.Hash(password),
                Role        = role,
                Status      = status ?? "Active",
                Permissions = permissions,
                TenantId    = role == "Tenant" ? tenantId : null
            });
            await _context.SaveChangesAsync();
            TempData["Success"] = $"User '{fullName}' created successfully.";
            return RedirectToAction(nameof(Index));
        }

        // ── AJAX: recent activity for a user ──────────────────────────
        [HttpGet]
        public async Task<IActionResult> GetUserActivity(int userId)
        {
            var logs = await _context.ActivityLogs
                .Where(a => a.UserId == userId)
                .OrderByDescending(a => a.Timestamp)
                .Take(20)
                .Select(a => new
                {
                    a.Action,
                    a.Details,
                    a.Platform,
                    timestamp = a.Timestamp.ToString("MMM d, yyyy  HH:mm")
                })
                .ToListAsync();
            return Json(logs);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user != null)
            {
                user.Status = user.Status == "Active" ? "Inactive" : "Active";
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateRole(int id, string role, string? permissions)
        {
            var user = await _context.Users.FindAsync(id);
            if (user != null)
            {
                user.Role = role;
                user.Permissions = permissions;
                await _context.SaveChangesAsync();
                TempData["Success"] = "User role updated.";
            }
            return RedirectToAction(nameof(Index));
        }

        public IActionResult ExportTenants()
        {
            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Tenants");
            var tenants = _context.Tenants.ToList();
            worksheet.Cell(1, 1).Value = "Full Name";
            worksheet.Cell(1, 2).Value = "Phone";

            for (int i = 0; i < tenants.Count; i++)
            {
                worksheet.Cell(i + 2, 1).Value = tenants[i].FullNameEn;
                worksheet.Cell(i + 2, 2).Value = tenants[i].Phone;
            }

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return File(stream.ToArray(),
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "TenantsReport.xlsx");
        }
    }
}

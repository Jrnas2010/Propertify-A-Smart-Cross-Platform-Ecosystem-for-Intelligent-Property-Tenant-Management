using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Propertify.Web.Data;
using Propertify.Web.Helpers;
using Propertify.Web.Models;

namespace Propertify.Web.Controllers
{
    [Authorize(Roles = "Owner")]
    public class TenantAccessController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TenantAccessController(ApplicationDbContext context) => _context = context;

        public async Task<IActionResult> ManageAccess()
        {
            var tenants = await _context.Tenants.ToListAsync();
            return View(tenants);
        }

        [HttpPost]
        public async Task<IActionResult> CreateAppAccount(int tenantId, string email, string password)
        {
            // Check if a user account already exists for this tenant
            if (_context.Users.Any(u => u.TenantId == tenantId))
            {
                TempData["Error"] = "هذا المستأجر لديه حساب مفعّل بالفعل.";
                return RedirectToAction(nameof(ManageAccess));
            }

            if (_context.Users.Any(u => u.Email == email))
            {
                TempData["Error"] = "البريد الإلكتروني مستخدم بالفعل.";
                return RedirectToAction(nameof(ManageAccess));
            }

            var tenant = await _context.Tenants.FindAsync(tenantId);
            if (tenant == null)
            {
                TempData["Error"] = "لم يتم العثور على المستأجر.";
                return RedirectToAction(nameof(ManageAccess));
            }

            var user = new User
            {
                FullName = tenant.FullNameAr,
                Email = email,
                Password = PasswordHelper.Hash(password),
                Role = "Tenant",
                TenantId = tenantId
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            TempData["Success"] = "تم تفعيل حساب المستأجر بنجاح. يمكنه الآن الدخول من تطبيق الهاتف.";
            return RedirectToAction(nameof(ManageAccess));
        }
    }
}

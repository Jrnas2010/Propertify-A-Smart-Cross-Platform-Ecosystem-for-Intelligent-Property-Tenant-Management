using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Propertify.Web.Data;
using Propertify.Web.Models;

namespace Propertify.Web.Controllers
{
    [Authorize(Roles = "Owner")]
    public class UtilityController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;

        public UtilityController(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        public IActionResult Index() => RedirectToAction(nameof(BillingList));

        [HttpGet]
        public async Task<IActionResult> CreateBill()
        {
            ViewBag.Units = await _context.Units
                .Include(u => u.Property)
                .OrderBy(u => u.Property!.Name).ThenBy(u => u.UnitNumber)
                .ToListAsync();

            ViewBag.Tenants = await _context.Tenants
                .Where(t => !t.IsArchived)
                .OrderBy(t => t.FirstNameEn)
                .ToListAsync();

            return View();
        }

        public async Task<IActionResult> BillingList(string? search, string? typeFilter)
        {
            var query = _context.UtilityBills
                .Include(b => b.Unit)
                .Include(b => b.Tenant)
                .OrderByDescending(b => b.IssueDate)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(b =>
                    b.Tenant.FirstNameAr.Contains(search) ||
                    b.Tenant.LastNameAr.Contains(search) ||
                    (b.Unit != null && b.Unit.UnitNumber.Contains(search)));

            if (!string.IsNullOrWhiteSpace(typeFilter) && typeFilter != "all")
                query = query.Where(b => b.ServiceType.ToLower() == typeFilter.ToLower());

            var bills = await query.ToListAsync();

            ViewBag.TotalPaid    = bills.Where(b => b.Status == "Paid").Sum(b => b.TotalAmount);
            ViewBag.TotalUnpaid  = bills.Where(b => b.Status == "Unpaid").Sum(b => b.TotalAmount);
            ViewBag.TotalOverdue = bills.Where(b => b.Status == "Overdue").Sum(b => b.TotalAmount);
            ViewBag.PaidCount    = bills.Count(b => b.Status == "Paid");
            ViewBag.UnpaidCount  = bills.Count(b => b.Status == "Unpaid");
            ViewBag.OverdueCount = bills.Count(b => b.Status == "Overdue");
            ViewBag.TotalCount   = bills.Count;
            ViewBag.Search       = search;
            ViewBag.TypeFilter   = typeFilter;

            return View(bills);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateBill(UtilityBill bill)
        {
            // Auto-resolve tenant from unit when not provided
            if (bill.TenantId == 0)
            {
                var tenant = await _context.Tenants.FirstOrDefaultAsync(t => t.UnitId == bill.UnitId);
                if (tenant != null) bill.TenantId = tenant.Id;
            }

            // If total not manually entered, calculate from readings using configured tariff
            if (bill.TotalAmount == 0 && bill.CurrentReading > bill.PreviousReading)
            {
                var tariff = _configuration.GetValue<decimal>("UtilitySettings:TariffPerUnit", 0.020m);
                bill.TotalAmount = (bill.CurrentReading - bill.PreviousReading) * tariff;
            }

            ModelState.Remove(nameof(bill.Unit));
            ModelState.Remove(nameof(bill.Tenant));

            if (ModelState.IsValid)
            {
                _context.UtilityBills.Add(bill);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Bill created successfully.";
                return RedirectToAction(nameof(BillingList));
            }

            ViewBag.Units = await _context.Units
                .Include(u => u.Property)
                .OrderBy(u => u.Property!.Name).ThenBy(u => u.UnitNumber)
                .ToListAsync();
            ViewBag.Tenants = await _context.Tenants
                .Where(t => !t.IsArchived)
                .OrderBy(t => t.FirstNameEn)
                .ToListAsync();
            return View(bill);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateBillStatus(int id, string status)
        {
            var bill = await _context.UtilityBills.FindAsync(id);
            if (bill != null)
            {
                bill.Status = status;
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(BillingList));
        }
    }
}

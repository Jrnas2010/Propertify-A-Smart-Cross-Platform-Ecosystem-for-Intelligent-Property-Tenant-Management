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

        public async Task<IActionResult> BillingList()
        {
            var bills = await _context.UtilityBills
                .Include(b => b.Unit)
                .Include(b => b.Tenant)
                .OrderByDescending(b => b.IssueDate)
                .ToListAsync();
            return View(bills);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateBill(UtilityBill bill)
        {
            if (ModelState.IsValid)
            {
                var tariff = _configuration.GetValue<decimal>("UtilitySettings:TariffPerUnit", 0.020m);
                bill.TotalAmount = (bill.CurrentReading - bill.PreviousReading) * tariff;
                bill.IssueDate = DateTime.Now;

                _context.UtilityBills.Add(bill);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(BillingList));
            }
            return View(bill);
        }
    }
}
